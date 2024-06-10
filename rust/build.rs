fn main() {
    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_dll_name("hf_tokenizers")
        .csharp_class_name("NativeMethods")
        .generate_csharp_file("../dotnet/Tokenizers.DotNet/NativeMethods.cs")
        .unwrap();
}
