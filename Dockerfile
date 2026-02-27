# https://github.com/mstorsjo/llvm-mingw
ARG RUST_VERSION=1.93.1

FROM rust:${RUST_VERSION}-bookworm AS rust

# Win X64
FROM rust AS winx64

RUN apt-get update && \
    apt-get install -qqy --no-install-recommends mingw-w64

RUN rustup target add x86_64-pc-windows-gnu

WORKDIR /src

COPY . .

WORKDIR /src/rust

RUN cargo build --target x86_64-pc-windows-gnu --release

# Win ARM64
FROM mstorsjo/llvm-mingw:20250319 AS winarm64

RUN apt-get update && \
    apt-get install -qqy --no-install-recommends g++ mingw-w64

# Install Rust
COPY --from=rust /usr/local/cargo /usr/local/cargo
COPY --from=rust /usr/local/rustup /usr/local/rustup
ENV PATH="$PATH:/usr/local/cargo/bin"

RUN rustup default stable
RUN rustup target add aarch64-pc-windows-gnullvm

WORKDIR /src

COPY . .

WORKDIR /src/rust

ENV CXXFLAGS="--stdlib=libc++"
RUN cargo build --target aarch64-pc-windows-gnullvm --release

# Linux X64
FROM rust AS linuxx64

# Install cross-compilation tools when building on non-x86_64 hosts (e.g., ARM64)
RUN if [ "$(dpkg --print-architecture)" != "amd64" ]; then \
      apt-get update && \
      apt-get install -qqy --no-install-recommends gcc-x86-64-linux-gnu g++-x86-64-linux-gnu libc6-dev-amd64-cross; \
    fi

RUN rustup target add x86_64-unknown-linux-gnu

WORKDIR /src

COPY . .

WORKDIR /src/rust
ENV CARGO_TARGET_X86_64_UNKNOWN_LINUX_GNU_LINKER=x86_64-linux-gnu-gcc \
    CC_x86_64_unknown_linux_gnu=x86_64-linux-gnu-gcc \
    CXX_x86_64_unknown_linux_gnu=x86_64-linux-gnu-g++
RUN cargo build --target x86_64-unknown-linux-gnu --release

# Linux ARM64
FROM rust AS linuxarm64

# Install cross-compilation tools when building on non-ARM64 hosts (e.g., x86_64)
RUN if [ "$(dpkg --print-architecture)" != "arm64" ]; then \
      apt-get update && \
      apt-get purge -y g++ && \
      apt-get install -qqy --no-install-recommends g++-aarch64-linux-gnu libc6-dev-arm64-cross; \
    fi

RUN rustup target add aarch64-unknown-linux-gnu

WORKDIR /src

COPY . .

WORKDIR /src/rust
ENV CARGO_TARGET_AARCH64_UNKNOWN_LINUX_GNU_LINKER=aarch64-linux-gnu-gcc \
    CC_aarch64_unknown_linux_gnu=aarch64-linux-gnu-gcc \
    CXX_aarch64_unknown_linux_gnu=aarch64-linux-gnu-g++
RUN cargo build --target aarch64-unknown-linux-gnu --release

# macOS X64
FROM rust AS osxx64

RUN cargo install cargo-zigbuild
RUN apt-get update && \
    apt-get install -qqy --no-install-recommends python3 python3-pip && \
    pip3 install --break-system-packages ziglang

RUN rustup target add x86_64-apple-darwin

WORKDIR /src

COPY . .

WORKDIR /src/rust
# libmimalloc-sys 0.1.44+ unconditionally includes <CommonCrypto/CommonCryptoError.h>
# when __APPLE__ is defined, but zig's cross-compilation sysroot lacks macOS framework headers.
# Provide minimal stubs so C code compiles; CCRandomGenerateBytes returns failure so
# mimalloc falls back to /dev/urandom for random seed initialization at runtime.
RUN mkdir -p /usr/local/include/CommonCrypto && \
    printf '#pragma once\n#include <stdint.h>\n#include <stddef.h>\ntypedef int32_t CCCryptorStatus;\ntypedef CCCryptorStatus CCRNGStatus;\nenum { kCCSuccess = 0 };\nstatic inline CCRNGStatus CCRandomGenerateBytes(void* b, size_t n) { return -1; }\n' \
    > /usr/local/include/CommonCrypto/CommonCryptoError.h && \
    printf '#pragma once\n#include <CommonCrypto/CommonCryptoError.h>\n' \
    > /usr/local/include/CommonCrypto/CommonRandom.h
ENV CFLAGS_x86_64_apple_darwin="-I/usr/local/include"
RUN cargo zigbuild --target x86_64-apple-darwin --release

# macOS ARM64
FROM rust AS osxarm64

RUN cargo install cargo-zigbuild
RUN apt-get update && \
    apt-get install -qqy --no-install-recommends python3 python3-pip && \
    pip3 install --break-system-packages ziglang

RUN rustup target add aarch64-apple-darwin

WORKDIR /src

COPY . .

WORKDIR /src/rust
RUN mkdir -p /usr/local/include/CommonCrypto && \
    printf '#pragma once\n#include <stdint.h>\n#include <stddef.h>\ntypedef int32_t CCCryptorStatus;\ntypedef CCCryptorStatus CCRNGStatus;\nenum { kCCSuccess = 0 };\nstatic inline CCRNGStatus CCRandomGenerateBytes(void* b, size_t n) { return -1; }\n' \
    > /usr/local/include/CommonCrypto/CommonCryptoError.h && \
    printf '#pragma once\n#include <CommonCrypto/CommonCryptoError.h>\n' \
    > /usr/local/include/CommonCrypto/CommonRandom.h
ENV CFLAGS_aarch64_apple_darwin="-I/usr/local/include"
RUN cargo zigbuild --target aarch64-apple-darwin --release

FROM rust

WORKDIR /src

SHELL ["/bin/bash", "-c"]

# Install nuget + dotnet
RUN apt-get update && \
    apt-get install -y nuget mono-complete dos2unix && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    nuget update -self

# Install PowerShell from GitHub release (no longer available via apt on Debian 12)
ARG PS_VERSION=7.4.7
RUN PS_ARCH=$(dpkg --print-architecture | sed 's/amd64/x64/' | sed 's/arm64/arm64/') && \
    wget -q https://github.com/PowerShell/PowerShell/releases/download/v${PS_VERSION}/powershell-${PS_VERSION}-linux-${PS_ARCH}.tar.gz -O /tmp/powershell.tar.gz && \
    mkdir -p /opt/microsoft/powershell/7 && \
    tar zxf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7 && \
    chmod +x /opt/microsoft/powershell/7/pwsh && \
    ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh && \
    rm /tmp/powershell.tar.gz

ENV PATH=$PATH:/root/.dotnet

COPY . .
# Clean
RUN dos2unix clear_all.ps1
RUN pwsh -File clear_all.ps1

COPY --from=winx64 /src/rust/target/x86_64-pc-windows-gnu/release/hf_tokenizers.dll /src/nuget/win-x64/hf_tokenizers.dll
COPY --from=winarm64 /src/rust/target/aarch64-pc-windows-gnullvm/release/hf_tokenizers.dll /src/nuget/win-arm64/hf_tokenizers.dll
COPY --from=linuxx64 /src/rust/target/x86_64-unknown-linux-gnu/release/libhf_tokenizers.so /src/nuget/linux-x64/libhf_tokenizers.so
COPY --from=linuxarm64 /src/rust/target/aarch64-unknown-linux-gnu/release/libhf_tokenizers.so /src/nuget/linux-arm64/libhf_tokenizers.so
COPY --from=osxx64 /src/rust/target/x86_64-apple-darwin/release/libhf_tokenizers.dylib /src/nuget/osx-x64/libhf_tokenizers.dylib
COPY --from=osxarm64 /src/rust/target/aarch64-apple-darwin/release/libhf_tokenizers.dylib /src/nuget/osx-arm64/libhf_tokenizers.dylib

# Build
RUN dos2unix build_dotnet.ps1
RUN pwsh -File build_dotnet.ps1

CMD ["cp", "-a", "/src/nuget/.", "/out/"]