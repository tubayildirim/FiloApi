path = "src/Filo.Infrastructure/Persistence/AppDbContextSeed.cs"
with open(path, "r") as f:
    text = f.read()

text = text.replace("using Serilog;\n", "")
text = text.replace("Log.Information", "Console.WriteLine")
text = text.replace("Log.Error", "Console.WriteLine")

with open(path, "w") as f:
    f.write(text)
