// lib.rs
// Tokenizer library based on Transformers.Tokenizer to use in dotnet project.

#[repr(C)]
pub struct ByteBuffer {
    ptr: *mut u8,
    length: i32,
    capacity: i32,
}

impl ByteBuffer {
    pub fn len(&self) -> usize {
        self.length
            .try_into()
            .expect("buffer length negative or overflowed")
    }

    pub fn from_vec(bytes: Vec<u8>) -> Self {
        let length = i32::try_from(bytes.len()).expect("buffer length cannot fit into a i32.");
        let capacity =
            i32::try_from(bytes.capacity()).expect("buffer capacity cannot fit into a i32.");

        // keep memory until call delete
        let mut v = std::mem::ManuallyDrop::new(bytes);

        Self {
            ptr: v.as_mut_ptr(),
            length,
            capacity,
        }
    }

    pub fn from_vec_struct<T: Sized>(bytes: Vec<T>) -> Self {
        let element_size = std::mem::size_of::<T>() as i32;

        let length = (bytes.len() as i32) * element_size;
        let capacity = (bytes.capacity() as i32) * element_size;

        let mut v = std::mem::ManuallyDrop::new(bytes);

        Self {
            ptr: v.as_mut_ptr() as *mut u8,
            length,
            capacity,
        }
    }

    pub fn destroy_into_vec(self) -> Vec<u8> {
        if self.ptr.is_null() {
            vec![]
        } else {
            let capacity: usize = self
                .capacity
                .try_into()
                .expect("buffer capacity negative or overflowed");
            let length: usize = self
                .length
                .try_into()
                .expect("buffer length negative or overflowed");

            unsafe { Vec::from_raw_parts(self.ptr, length, capacity) }
        }
    }

    pub fn destroy_into_vec_struct<T: Sized>(self) -> Vec<T> {
        if self.ptr.is_null() {
            vec![]
        } else {
            let element_size = std::mem::size_of::<T>() as i32;
            let length = (self.length * element_size) as usize;
            let capacity = (self.capacity * element_size) as usize;

            unsafe { Vec::from_raw_parts(self.ptr as *mut T, length, capacity) }
        }
    }

    pub fn destroy(self) {
        drop(self.destroy_into_vec());
    }
}

#[no_mangle]
pub extern "C" fn alloc_u8_string() -> *mut ByteBuffer {
    let str = format!("foo bar baz");
    let buf = ByteBuffer::from_vec(str.into_bytes());
    Box::into_raw(Box::new(buf))
}

#[no_mangle]
pub unsafe extern "C" fn free_u8_string(buffer: *mut ByteBuffer) {
    let buf = Box::from_raw(buffer);
    // drop inner buffer, if you need String, use String::from_utf8_unchecked(buf.destroy_into_vec()) instead.
    buf.destroy();
}

#[no_mangle]
pub unsafe extern "C" fn csharp_to_rust_string(utf16_str: *const u16, utf16_len: i32) {
    let slice = std::slice::from_raw_parts(utf16_str, utf16_len as usize);
    let str = String::from_utf16(slice).unwrap();
    println!("{}", str);
}

#[no_mangle]
pub unsafe extern "C" fn csharp_to_rust_u32_array(buffer: *const u32, len: i32) {
    let slice = std::slice::from_raw_parts(buffer, len as usize);
    let vec = slice.to_vec();
    println!("{:?}", vec);
}

// Tokenizer stuff starts here
use once_cell::unsync::Lazy;
use std::collections::HashMap;
use std::fmt;
use tokenizers::tokenizer::Tokenizer;
use uuid::Uuid;

#[repr(C)]
pub struct TokenizerInfo {
    pub tokenizer_path: String,  // path to tokenizer.json
    pub library_version: String, // From cargo.toml
    pub tokenizer_id: String,    // Session ID(UUID) of the tokenizer
}

impl fmt::Debug for TokenizerInfo {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(
            f,
            "TokenizerInfo {{ tokenizer_path: {}, library_version: {} }}",
            self.tokenizer_path, self.library_version
        )
    }
}

static mut TOKENIZER_SESSION: Lazy<HashMap<String, TokenizerInfo>> = Lazy::new(|| HashMap::new());

static mut TOKENIZER_DB: Lazy<HashMap<String, Tokenizer>> = Lazy::new(|| HashMap::new());

#[no_mangle]
pub unsafe extern "C" fn tokenizer_initialize(
    utf16_path: *const u16,
    utf16_path_len: i32,
) -> *mut ByteBuffer {
    // Initialize the GLOBAL_TOKENIZER_INFO
    let slice = std::slice::from_raw_parts(utf16_path, utf16_path_len as usize);
    let utf8_path = String::from_utf16(slice).unwrap();

    let id = Uuid::new_v4().to_string();
    // println!("Session ID: {}", id);

    let version = env!("CARGO_PKG_VERSION");
    let info = TokenizerInfo {
        tokenizer_path: utf8_path.clone(),
        library_version: version.to_string(),
        tokenizer_id: id.clone(),
    };

    // Add to TOKENIZER_SESSION
    TOKENIZER_SESSION.insert(id.clone(), info);

    // Initialize TOKENIZER_DB if not already initialized
    let path = utf8_path.clone();
    let tokenizer = Tokenizer::from_file(&path).unwrap();
    TOKENIZER_DB.insert(id.clone(), tokenizer);

    let session_id = id.clone();
    Box::into_raw(Box::new(ByteBuffer::from_vec(session_id.into_bytes())))
}

#[no_mangle]
pub unsafe extern "C" fn tokenizer_encode(
    _session_id: *const u16,
    _session_id_len: i32,
    _text: *const u16,
    _text_len: i32,
) -> *mut ByteBuffer {
    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = String::from_utf16(slice_session_id).unwrap();
    let slice_text = std::slice::from_raw_parts(_text, _text_len as usize);
    let text = String::from_utf16(slice_text).unwrap();

    // Retrieve the tokenizer associated with the session ID
    let tokenizer = TOKENIZER_DB
        .get(&session_id)
        .cloned()
        .unwrap_or_else(|| panic!("Tokenizer for session ID '{}' not found.", session_id));

    // Encode the text
    let encoded_result = tokenizer.encode(text.clone(), true);
    let encoded_tokens = match encoded_result {
        Ok(encoded) => encoded,
        Err(err) => panic!("{}", err),
    };
    let token_ids = encoded_tokens
        .get_ids()
        .iter()
        .map(|&i| i as u32)
        .collect::<Vec<u32>>();

    // Convert the token IDs to a ByteBuffer
    let buf = ByteBuffer::from_vec_struct(token_ids);
    Box::into_raw(Box::new(buf))
}

// Returns u8string. Caller must free the memory
#[no_mangle]
pub unsafe extern "C" fn tokenizer_decode(
    _session_id: *const u16,
    _session_id_len: i32,
    _token_ids: *const u32,
    _token_ids_len: i32,
) -> *mut ByteBuffer {
    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = String::from_utf16(slice_session_id).unwrap();
    let slice_token_ids = std::slice::from_raw_parts(_token_ids, _token_ids_len as usize);
    let token_ids_vec: Vec<u32> = slice_token_ids.iter().copied().collect();

    // Retrieve the tokenizer associated with the session ID
    let tokenizer = TOKENIZER_DB
        .get(&session_id)
        .cloned()
        .unwrap_or_else(|| panic!("Tokenizer for session ID '{}' not found.", session_id));

    // Decode the tokens
    let decoded_result = tokenizer.decode(&token_ids_vec, true);
    let decoded_text = match decoded_result {
        Ok(decoded) => decoded,
        Err(e) => {
            eprintln!("Error decoding tokens: {:?}", e);
            return Box::into_raw(Box::new(ByteBuffer::from_vec(vec![]))); // Return empty string on error
        }
    };

    // Convert the decoded text to bytes and return as ByteBuffer
    let buf = ByteBuffer::from_vec(decoded_text.into_bytes());
    Box::into_raw(Box::new(buf))
}

#[no_mangle]
pub unsafe extern "C" fn get_version(
    _session_id: *const u16,
    _session_id_len: i32,
) -> *mut ByteBuffer {
    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = String::from_utf16(slice_session_id).unwrap();

    // Retrieve the TokenizerInfo associated with the session ID
    let session_info = TOKENIZER_SESSION
        .get(&session_id)
        .clone()
        .unwrap_or_else(|| panic!("Session info for session ID '{}' not found.", session_id));

    // Get the library version from the TokenizerInfo
    let version = session_info.library_version.clone();

    // Convert the version string to bytes and return as ByteBuffer
    let buf = ByteBuffer::from_vec(version.into_bytes());
    Box::into_raw(Box::new(buf))
}
