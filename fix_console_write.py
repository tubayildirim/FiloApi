path = "src/Filo.Infrastructure/Persistence/AppDbContextSeed.cs"
with open(path, "r") as f:
    text = f.read()

text = text.replace('Console.WriteLine(ex, "An error occurred while migrating or seeding the database.");', 'Console.WriteLine($"An error occurred while migrating or seeding the database. {ex.Message}");')

with open(path, "w") as f:
    f.write(text)
