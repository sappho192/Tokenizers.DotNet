#!/bin/bash

set -e
base_dir="nuget"

# Retrieve the current Git branch and commit hash
branch=$(git rev-parse --abbrev-ref HEAD)
commit=$(git rev-parse HEAD)

# Iterate over each subdirectory in the base directory
for dir in "$base_dir"/*/; do
    # Check if it is a directory
    if [[ -d "$dir" ]]; then
        echo "Entering directory: $dir"
        
        # Change to the subdirectory
        pushd "$dir" > /dev/null
        
        # Find the first .nuspec file in the current subdirectory
        nuspec_file=$(find . -maxdepth 1 -type f -name "*.nuspec" | head -n 1)
        
        # Check if a .nuspec file was found
        if [[ -n "$nuspec_file" ]]; then
            echo "Found nuspec file: $nuspec_file"

            nuget pack "$nuspec_file"
        else
            echo "No .nuspec file found in $dir"
        fi
        
        # Return to the original directory
        popd > /dev/null
    else
        echo "$dir is not a valid directory."
    fi
done