# F# Crop Variety Patent Checker

Simple F# console app to check crop variety patent status using the Gossortrf registry.

## Usage

1. Put your `input.csv` in the project folder.
2. Build and run the project:

```bash
dotnet build
dotnet run
```

3. Check results in console or in `result.txt`.

## Input CSV Format

```
selection,variety
crop1,variety1
crop2,variety2
```

- `selection` — region or culture
- `variety` — crop variety name

## Notes

- Supports UTF-8 and Windows-1251 encoded CSV files.
- Clears `result.txt` before writing new results.
- Uses semicolon or comma as separator.

