import re
files = [
    "src/Filo.Application/Features/VehicleToll/Queries/GetPagedVehicleTollQuery.cs",
    "src/Filo.Application/Features/VehicleTrafficFine/Queries/GetPagedVehicleTrafficFineQuery.cs",
    "src/Filo.Application/Features/VehicleInsurance/Queries/GetPagedVehicleInsuranceQuery.cs",
    "src/Filo.Application/Features/VehicleFuel/Queries/GetPagedVehicleFuelQuery.cs",
    "src/Filo.Application/Features/Vehicles/Queries/GetPagedVehiclesQuery.cs",
    "src/Filo.Application/Features/VehicleMaintenance/Queries/GetPagedVehicleMaintenanceQuery.cs",
    "src/Filo.Application/Features/VehicleService/Queries/GetPagedVehicleServiceQuery.cs"
]

for path in files:
    with open(path, "r") as f:
        content = f.read()

    # Fix expression body constructor
    # Find: public SomeHandler(...) => _unitOfWork = unitOfWork;\n        _rbacService = rbacService;
    content = re.sub(r"public (\w+Handler)\((.*?)\)\s*=>\s*_unitOfWork\s*=\s*unitOfWork;\s*_rbacService\s*=\s*rbacService;",
                     r"public \1(\2) { _unitOfWork = unitOfWork; _rbacService = rbacService; }",
                     content)

    with open(path, "w") as f:
        f.write(content)
