FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    jq \
    git \
    curl \
    ca-certificates \
    gnupg \
  && rm -rf /var/lib/apt/lists/*

# Install Node.js LTS (20.x)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# Install Claude CLI
RUN npm install -g @anthropic-ai/claude-code

# Copy workspace
COPY . /workspace
WORKDIR /workspace

# Restore .NET dependencies
RUN dotnet restore Pemberton.Shareclass.Hedging.Prototype/Pemberton.Shareclass.Hedging.Prototype.sln

# Make entrypoint executable
RUN chmod +x scripts/ralph/entrypoint.sh scripts/ralph/ralph.sh

ENTRYPOINT ["scripts/ralph/entrypoint.sh"]
