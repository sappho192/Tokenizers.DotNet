FROM rust:1.85.0-bookworm AS base

# Install powershell
SHELL ["/bin/bash", "-c"]
RUN apt-get update && \
    apt-get install -y wget && \
    source /etc/os-release && \
    wget -q https://packages.microsoft.com/config/debian/$VERSION_ID/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y powershell 

# Install nuget + dotnet
RUN apt-get update && \
    apt-get install -y nuget && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0

ENV PATH=$PATH:/root/.dotnet

WORKDIR /src
COPY . .
WORKDIR /src/rust

# Windows x64
FROM base AS winX64Builder

RUN apt-get update && apt-get install -y mingw-w64

RUN rustup target add x86_64-pc-windows-gnu

RUN cargo build --target x86_64-pc-windows-gnu --release

FROM base AS winARM64Builder

RUN rustup target add aarch64-pc-windows-gnullvm

# Add custom Linux x64 -> Windows Arm64 compiler
WORKDIR /compiler
ARG COMPILER="llvm-mingw-20250319-msvcrt-ubuntu-20.04-x86_64"
RUN wget https://github.com/mstorsjo/llvm-mingw/releases/download/20250319/${COMPILER}.tar.xz
RUN tar xf ${COMPILER}.tar.xz
RUN rm ${COMPILER}.tar.xz
RUN cp -r ${COMPILER}/generic-w64-mingw32/include /usr/local
ENV PATH="$PATH:/compiler/${COMPILER}/bin"
ENV CXX=aarch64-w64-mingw32-clang++
ENV CC=aarch64-w64-mingw32-clang
ENV CXXFLAGS="-isystem /compiler/${COMPILER}/include/c++/v1 --sysroot=/compiler/${COMPILER} --stdlib=libc++"

WORKDIR /src/rust
RUN cargo build --target aarch64-pc-windows-gnullvm --release

FROM rust:1.85.0-bookworm

WORKDIR /out
WORKDIR /tmp

COPY --from=winX64Builder /src/rust/target/x86_64-pc-windows-gnu/release/hf_tokenizers.dll /tmp/x64/hf_tokenizers.dll
COPY --from=winARM64Builder /src/rust/target/aarch64-pc-windows-gnullvm/release/hf_tokenizers.dll /tmp/arm64/hf_tokenizers.dll

CMD ["cp", "-a", "/tmp/.", "/out/"]