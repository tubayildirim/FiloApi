import glob
import re

features_dir = "src/Filo.Application/Features"
files = [
    "Vehicles/Queries/GetPagedVehiclesQuery.cs",
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

    # If predicate is not declared before 'var allowedIds', let's declare it
    if "VehiclesQuery" in path:
        ent = "Filo.Domain.Entities.Vehicle"
    elif "Fuel" in path: ent = "Filo.Domain.Entities.VehicleFuel"
    elif "Insurance" in path: ent = "Filo.Domain.Entities.VehicleInsurance"
    elif "Maintenance" in path: ent = "Filo.Domain.Entities.VehicleMaintenance"
    elif "Service" in path: ent = "Filo.Domain.Entities.VehicleService"
    elif "Toll" in path: ent = "Filo.Domain.Entities.VehicleToll"
    elif "TrafficFine" in path: ent = "Filo.Domain.Entities.VehicleTrafficFine"

    # Some files might have `System.Linq.Expressions.Expression<Func<...>>? predicate = null;` already.
    # We will just declare it above `var allowedIds` if we don't find the word predicate before it.
    
    # Actually, simpler: just search for `var allowedIds = ` and replace with the declaration
    # but only if it's not `VehiclesQuery.cs` since it already has it.
    if "VehiclesQuery" not in path:
        # replace `if (predicate == null)` with declaration
        if "System.Linq.Expressions.Expression<Func<" not in content:
            decl = f"System.Linq.Expressions.Expression<Func<{ent}, bool>>? predicate = null;"
            content = content.replace("var allowedIds = await _rbacService.GetAllowedVehicleIdsAsync();",
                                      f"{decl}\n            var allowedIds = await _rbacService.GetAllowedVehicleIdsAsync();")
    
    with open(path, "w") as f:
        f.write(content)

