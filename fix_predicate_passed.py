import glob
import re

features_dir = "src/Filo.Application/Features"
files = [
    "VehicleFuel/Queries/GetPagedVehicleFuelQuery.cs",
    "VehicleInsurance/Queries/GetPagedVehicleInsuranceQuery.cs",
    "VehicleMaintenance/Queries/GetPagedVehicleMaintenanceQuery.cs",
    "VehicleService/Queries/GetPagedVehicleServiceQuery.cs",
    "VehicleToll/Queries/GetPagedVehicleTollQuery.cs",
    "VehicleTrafficFine/Queries/GetPagedVehicleTrafficFineQuery.cs"
]

for rel_path in files:
    path = f"{features_dir}/{rel_path}"
    with open(path, "r") as f:
        content = f.read()

    # Find the GetPagedAsync call and replace `null` with `predicate`
    # e.g., GetPagedAsync(pageNumber, pageSize, null, orderBy)
    # Note: Toll and TrafficFine and Insurance use `q => q.OrderByDescending(...)` instead of `orderBy`.
    
    # regex to replace GetPagedAsync(pageNumber, pageSize, null,
    # with GetPagedAsync(pageNumber, pageSize, predicate,
    
    content = re.sub(r"GetPagedAsync\((pageNumber,\s*pageSize,\s*)null(,)", r"GetPagedAsync(\1predicate\2", content)
    
    with open(path, "w") as f:
        f.write(content)
        print("Fixed predicate passing in", path)

