# Start with debian 12
FROM debian:12.10

# Update apt-get once
RUN apt-get update

# Install core packages necessary for the rest of the way
RUN apt-get install -y build-essential
RUN apt-get install -y cmake
RUN apt-get install -y curl
RUN apt-get install -y make
RUN apt-get install -y wget

# Install Rust (do this first so that adding apt-get packages can skip this step in the future)
RUN curl https://sh.rustup.rs -sSf | sh -s -- -y

# Install cargo packages (again, do this before the apt-get packages because it takes forever)
## Lune
RUN /bin/bash -c "source $HOME/.cargo/env && cargo install lune --version 0.9.0  --locked"

# Add apt-get sources
## Dotnet
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Update apt-get with all the sources
RUN apt-get update

# Install develop-time tools from apt-get
RUN apt-get install -y dotnet-sdk-8.0
RUN apt-get install -y git
RUN apt-get install -y golang-go
RUN apt-get install -y python3

# Clean up
RUN apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Verify installations
RUN /bin/bash -c "source $HOME/.cargo/env && cargo --version"
RUN /bin/bash -c "dotnet --version"
RUN /bin/bash -c "git --version"
RUN /bin/bash -c "go version"
RUN /bin/bash -c "source $HOME/.cargo/env && lune --version"
