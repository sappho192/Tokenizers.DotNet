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
use lazy_static::lazy_static;
use std::fs::File;
use std::io::Read;
use tokenizers::tokenizer::Tokenizer;

// Initialize the tokenizer
lazy_static! {
    static ref TOKENIZER: Tokenizer = {
        // Read the file path from the text file
        let mut file = File::open("tokenizer.path.txt").expect("Failed to find tokenizer.path.txt file");
        let mut path = String::new();
        file.read_to_string(&mut path).expect("Failed to read tokenizer.path.txt file");

        Tokenizer::from_file(path).unwrap()
    };
}

// Returns u8string. Caller must free the memory
#[no_mangle]
pub unsafe extern "C" fn tokenizer_decode(buffer: *const u32, len: i32) -> *mut ByteBuffer {
    let slice = std::slice::from_raw_parts(buffer, len as usize);
    // let vec = slice.to_vec();
    // println!("{:?}", vec);
    let decoded = TOKENIZER.decode(slice, true);
    if decoded.is_err() {
        // return empty string
        return Box::into_raw(Box::new(ByteBuffer::from_vec(vec![])));
    }
    let str = decoded.unwrap();
    // println!("{:?}", str);

    let buf = ByteBuffer::from_vec(str.into_bytes());
    Box::into_raw(Box::new(buf))
}
