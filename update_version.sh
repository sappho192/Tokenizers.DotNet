#!/bin/bash

version=$(cat "./NATIVE_LIB_VERSION.txt")
echo "Version: $version"

sed -E 's/^(version\s*=\s*")[^"]*/\1'"$version"'/' "rust/Cargo.toml"

# Update version in every nuspec
find nuget -type f -name "*.nuspec" | while IFS= read -r nuspecFile; do
    echo "Processing: $nuspecFile"
    # Use sed to replace the content within the <version> tags
    sed -i.bak -E "s/(<version>)[^<]*(<\/version>)/\1$version\2/" "$nuspecFile"
done