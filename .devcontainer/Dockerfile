FROM debian:12.10

RUN apt-get update && \
    apt-get install -y build-essential cmake curl git make python3 golang-go

### Install Rust
RUN curl https://sh.rustup.rs -sSf | sh -s -- -y

### Clean up
RUN apt-get clean && \
    rm -rf /var/lib/apt/lists/*

## Verify installations
RUN /bin/bash -c "source $HOME/.cargo/env && cargo --version && git --version && go version"

## Install cargo packages
### Lune
RUN /bin/bash -c "source $HOME/.cargo/env && cargo install lune --version 0.9.0  --locked"
