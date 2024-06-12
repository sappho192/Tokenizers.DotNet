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
use once_cell::sync::OnceCell;
use std::fmt;
use tokenizers::tokenizer::Tokenizer;

#[repr(C)]
pub struct TokenizerInfo {
    pub tokenizer_path: String,  // path to tokenizer.json
    pub library_version: String, // From cargo.toml
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

static GLOBAL_TOKENIZER: OnceCell<Tokenizer> = OnceCell::new();
static GLOBAL_TOKENIZER_INFO: OnceCell<TokenizerInfo> = OnceCell::new();

#[no_mangle]
pub unsafe extern "C" fn tokenizer_initialize(utf16_path: *const u16, utf16_path_len: i32) {
    // Initialize the GLOBAL_TOKENIZER_INFO
    let slice = std::slice::from_raw_parts(utf16_path, utf16_path_len as usize);
    let utf8_path = String::from_utf16(slice).unwrap();

    let version = env!("CARGO_PKG_VERSION");
    let info = TokenizerInfo {
        tokenizer_path: utf8_path.clone(),
        library_version: version.to_string(),
    };
    GLOBAL_TOKENIZER_INFO
        .set(info)
        .expect("Failed to set GLOBAL_TOKENIZER_INFO");

    // Initialize the GLOBAL_TOKENIZER
    let path = utf8_path.clone();
    let tokenizer = Tokenizer::from_file(&path).unwrap();
    GLOBAL_TOKENIZER
        .set(tokenizer)
        .expect("Failed to set GLOBAL_TOKENIZER");
}

// Returns u8string. Caller must free the memory
#[no_mangle]
pub unsafe extern "C" fn tokenizer_decode(buffer: *const u32, len: i32) -> *mut ByteBuffer {
    let slice = std::slice::from_raw_parts(buffer, len as usize);
    // let vec = slice.to_vec();
    // println!("{:?}", vec);
    let tokenizer = GLOBAL_TOKENIZER.get().unwrap();
    let decoded = tokenizer.decode(slice, true);
    if decoded.is_err() {
        // return empty string
        return Box::into_raw(Box::new(ByteBuffer::from_vec(vec![])));
    }
    let str = decoded.unwrap();
    // println!("{:?}", str);

    let buf = ByteBuffer::from_vec(str.into_bytes());
    Box::into_raw(Box::new(buf))
}

// #[no_mangle]
// pub unsafe extern "C" fn get_version() -> *mut ByteBuffer {
//     const VERSION: &str = env!("CARGO_PKG_VERSION");
//     // Copy VERSION to new String
//     let buf = ByteBuffer::from_vec(VERSION.to_string().into_bytes());
//     Box::into_raw(Box::new(buf))
// }
