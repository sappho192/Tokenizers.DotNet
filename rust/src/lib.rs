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
use std::collections::HashMap;
use std::fmt;
use std::sync::{LazyLock, RwLock};
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

// Error code starts from 20001, since Windows error codes ends with 16000
#[repr(C)]
pub enum TokenizerErrorCode {
    Success = 0,
    InvalidInput = 20001,
    InitializationError = 20002,
    InvalidSessionId = 20003,
    EncodingError = 20004,
    DecodingError = 20005,
}


#[repr(C)]
pub struct TokenizerResult {
    pub error_code: TokenizerErrorCode,
    pub data: *mut ByteBuffer // Could be null if error occured
}

static mut LAST_ERROR_MESSAGE: String = String::new();

static TOKENIZER_SESSION: LazyLock<RwLock<HashMap<String, TokenizerInfo>>> =
    LazyLock::new(|| RwLock::new(HashMap::new()));

static TOKENIZER_DB: LazyLock<RwLock<HashMap<String, Tokenizer>>> =
    LazyLock::new(|| RwLock::new(HashMap::new()));

#[no_mangle]
pub unsafe extern "C" fn get_last_error_message() -> *mut ByteBuffer {
    let buf = ByteBuffer::from_vec(LAST_ERROR_MESSAGE.clone().into_bytes());
    Box::into_raw(Box::new(buf))
}

#[no_mangle]
pub unsafe extern "C" fn tokenizer_initialize(
    utf16_path: *const u16,
    utf16_path_len: i32,
) -> TokenizerResult {
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
    TOKENIZER_SESSION.write().unwrap().insert(id.clone(), info);

    // Initialize TOKENIZER_DB if not already initialized
    let tokenizer = match Tokenizer::from_file(&utf8_path) {
        Ok(t) => t,
        Err(e) => {
            LAST_ERROR_MESSAGE = format!("Failed to load tokenizer: {}", e);
            return TokenizerResult {
                error_code: TokenizerErrorCode::InitializationError,
                data: std::ptr::null_mut(),
            };
        }
    };
    TOKENIZER_DB.write().unwrap().insert(id.clone(), tokenizer);

    let session_id = id.clone();
    TokenizerResult {
        error_code: TokenizerErrorCode::Success,
        data: Box::into_raw(Box::new(ByteBuffer::from_vec(session_id.into_bytes()))),
    }
}

#[no_mangle]
pub unsafe extern "C" fn tokenizer_encode(
    _session_id: *const u16,
    _session_id_len: i32,
    _text: *const u16,
    _text_len: i32,
) -> TokenizerResult {
    if _session_id.is_null() || _session_id_len <= 0 {
        LAST_ERROR_MESSAGE = "Invalid session ID pointer".to_string();
        return TokenizerResult {
            error_code: TokenizerErrorCode::InvalidInput,
            data: std::ptr::null_mut(),
        };
    }

    if _text.is_null() && _text_len > 0 {
        LAST_ERROR_MESSAGE = "Invalid text pointer".to_string();
        return TokenizerResult {
            error_code: TokenizerErrorCode::InvalidInput,
            data: std::ptr::null_mut(),
        };
    }

    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = match String::from_utf16(slice_session_id) {
        Ok(id) => id,
        Err(_) => {
            LAST_ERROR_MESSAGE = "Invalid UTF-16 session ID string".to_string();
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidInput,
                data: std::ptr::null_mut(),
            };
        }
    };
    let text = if _text.is_null() {
        String::new()  // Handle empty text case
    } else {
        let slice_text = std::slice::from_raw_parts(_text, _text_len as usize);
        match String::from_utf16(slice_text) {
            Ok(t) => t,
            Err(_) => {
                LAST_ERROR_MESSAGE = "Invalid UTF-16 text string".to_string();
                return TokenizerResult {
                    error_code: TokenizerErrorCode::InvalidInput,
                    data: std::ptr::null_mut(),
                };
            }
        }
    };

    // Retrieve the tokenizer associated with the session ID and invoke it
    let result = match TOKENIZER_DB.read().unwrap().get(&session_id) {
        Some(t) => t.encode(text, true),
        None => {
            LAST_ERROR_MESSAGE = format!("Tokenizer for session ID '{}' not found", session_id);
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidSessionId,
                data: std::ptr::null_mut(),
            };
        }
    };

    // Encode the text
    let encoded_tokens = match result {
        Ok(encoded) => encoded,
        Err(err) => {
            LAST_ERROR_MESSAGE = format!("Error encoding text: {}", err);
            return TokenizerResult {
                error_code: TokenizerErrorCode::EncodingError,
                data: std::ptr::null_mut(),
            };
        }
    };
    let token_ids = encoded_tokens
        .get_ids()
        .iter()
        .map(|&i| i as u32)
        .collect::<Vec<u32>>();

    // Convert the token IDs to a ByteBuffer
    TokenizerResult {
        error_code: TokenizerErrorCode::Success,
        data: Box::into_raw(Box::new(ByteBuffer::from_vec_struct(token_ids))),
    }
}

// Returns u8string. Caller must free the memory
#[no_mangle]
pub unsafe extern "C" fn tokenizer_decode(
    _session_id: *const u16,
    _session_id_len: i32,
    _token_ids: *const u32,
    _token_ids_len: i32,
) -> TokenizerResult {
    if _session_id.is_null() || _session_id_len <= 0 {
        LAST_ERROR_MESSAGE = "Invalid session ID pointer".to_string();
        return TokenizerResult {
            error_code: TokenizerErrorCode::InvalidInput,
            data: std::ptr::null_mut(),
        };
    }

    if _token_ids.is_null() && _token_ids_len > 0 {
        LAST_ERROR_MESSAGE = "Invalid token IDs pointer".to_string();
        return TokenizerResult {
            error_code: TokenizerErrorCode::InvalidInput,
            data: std::ptr::null_mut(),
        };
    }

    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = match String::from_utf16(slice_session_id) {
        Ok(id) => id,
        Err(_) => {
            LAST_ERROR_MESSAGE = "Invalid UTF-16 session ID string".to_string();
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidInput,
                data: std::ptr::null_mut(),
            };
        }
    };
    let token_ids_vec = if _token_ids.is_null() {
        Vec::new()  // Handle empty tokens case
    } else {
        let slice_token_ids = std::slice::from_raw_parts(_token_ids, _token_ids_len as usize);
        slice_token_ids.iter().copied().collect::<Vec<u32>>()
    };

    // Retrieve the tokenizer associated with the session ID and invoke it
    let result = match TOKENIZER_DB.read().unwrap().get(&session_id) {
        Some(t) => t.decode(&token_ids_vec, true),
        None => {
            LAST_ERROR_MESSAGE = format!("Tokenizer for session ID '{}' not found", session_id);
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidSessionId,
                data: std::ptr::null_mut(),
            };
        }
    };

    // Decode the tokens
    let decoded_text = match result {
        Ok(decoded) => decoded,
        Err(err) => {
            LAST_ERROR_MESSAGE = format!("Error decoding tokens: {}", err);
            return TokenizerResult {
                error_code: TokenizerErrorCode::DecodingError,
                data: std::ptr::null_mut(),
            };
        }
    };

    // Return success with decoded text as ByteBuffer
    TokenizerResult {
        error_code: TokenizerErrorCode::Success,
        data: Box::into_raw(Box::new(ByteBuffer::from_vec(decoded_text.into_bytes()))),
    }
}

#[no_mangle]
pub unsafe extern "C" fn get_version(
    _session_id: *const u16,
    _session_id_len: i32,
) -> TokenizerResult {
    if _session_id.is_null() || _session_id_len <= 0 {
        LAST_ERROR_MESSAGE = "Invalid session ID pointer".to_string();
        return TokenizerResult {
            error_code: TokenizerErrorCode::InvalidInput,
            data: std::ptr::null_mut(),
        };
    }

    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = match String::from_utf16(slice_session_id) {
        Ok(id) => id,
        Err(_) => {
            LAST_ERROR_MESSAGE = "Invalid UTF-16 session ID string".to_string();
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidInput,
                data: std::ptr::null_mut(),
            };
        }
    };

    // Retrieve the TokenizerInfo associated with the session ID and and get the version
    let version = match TOKENIZER_SESSION.read().unwrap().get(&session_id) {
        Some(info) => info.library_version.clone(),
        None => {
            LAST_ERROR_MESSAGE = format!("Session info for session ID '{}' not found", session_id);
            return TokenizerResult {
                error_code: TokenizerErrorCode::InvalidSessionId,
                data: std::ptr::null_mut(),
            };
        }
    };

    // Return success with version as ByteBuffer
    TokenizerResult {
        error_code: TokenizerErrorCode::Success,
        data: Box::into_raw(Box::new(ByteBuffer::from_vec(version.into_bytes()))),
    }
}

#[no_mangle]
pub unsafe extern "C" fn tokenizer_cleanup(
    _session_id: *const u16,
    _session_id_len: i32,
) -> TokenizerErrorCode {
    if _session_id.is_null() || _session_id_len <= 0 {
        LAST_ERROR_MESSAGE = "Invalid session ID pointer".to_string();
        return TokenizerErrorCode::InvalidInput;
    }

    // Convert session ID from UTF-16 to Rust string
    let slice_session_id = std::slice::from_raw_parts(_session_id, _session_id_len as usize);
    let session_id = match String::from_utf16(slice_session_id) {
        Ok(id) => id,
        Err(_) => {
            LAST_ERROR_MESSAGE = "Invalid UTF-16 session ID string".to_string();
            return TokenizerErrorCode::InvalidInput;
        }
    };

    // Remove tokenizer and session info
    let removed_tokenizer = TOKENIZER_DB.write().unwrap().remove(&session_id);
    let removed_session = TOKENIZER_SESSION.write().unwrap().remove(&session_id);

    if removed_tokenizer.is_none() || removed_session.is_none() {
        LAST_ERROR_MESSAGE = format!("Session ID '{}' not found", session_id);
        return TokenizerErrorCode::InvalidSessionId;
    }

    TokenizerErrorCode::Success
}
