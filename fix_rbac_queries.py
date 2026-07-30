import glob

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

    # Add using
    if "using Filo.Application.Common.Interfaces;" not in content:
        content = "using Filo.Application.Common.Interfaces;\n" + content

    handler_name = rel_path.split("/")[-1].replace(".cs", "Handler")
    
    # 1. Add field
    if "_rbacService" not in content:
        content = content.replace("private readonly IUnitOfWork _unitOfWork;", "private readonly IUnitOfWork _unitOfWork;\n    private readonly IRbacService _rbacService;")
        
        # 2. Add to constructor
        if "_cacheService" in content:
            # Vehicles uses cache service
            content = content.replace(f"public {handler_name}(IUnitOfWork unitOfWork, ICacheService cacheService)", f"public {handler_name}(IUnitOfWork unitOfWork, ICacheService cacheService, IRbacService rbacService)")
            content = content.replace("_cacheService = cacheService;", "_cacheService = cacheService;\n        _rbacService = rbacService;")
        elif "IMapper" in content:
            # Others use mapper
            content = content.replace(f"public {handler_name}(IUnitOfWork unitOfWork, IMapper mapper)", f"public {handler_name}(IUnitOfWork unitOfWork, IMapper mapper, IRbacService rbacService)")
            content = content.replace("_mapper = mapper;", "_mapper = mapper;\n        _rbacService = rbacService;")
        else:
            # No mapper?
            content = content.replace(f"public {handler_name}(IUnitOfWork unitOfWork)", f"public {handler_name}(IUnitOfWork unitOfWork, IRbacService rbacService)")
            content = content.replace("_unitOfWork = unitOfWork;", "_unitOfWork = unitOfWork;\n        _rbacService = rbacService;")
            
        # 3. Inject logic inside Handle method
        # Before 'var items = await' or 'var (items, count) = await'
        if "VehiclesQuery" in path:
            ent = "Vehicle"
            vid = "Id"
        elif "Fuel" in path: ent, vid = "VehicleFuel", "VehicleId"
        elif "Insurance" in path: ent, vid = "VehicleInsurance", "VehicleId"
        elif "Maintenance" in path: ent, vid = "VehicleMaintenance", "VehicleId"
        elif "Service" in path: ent, vid = "VehicleService", "VehicleId"
        elif "Toll" in path: ent, vid = "VehicleToll", "VehicleId"
        elif "TrafficFine" in path: ent, vid = "VehicleTrafficFine", "VehicleId"
        
        logic = f"""
            var allowedIds = await _rbacService.GetAllowedVehicleIdsAsync();
            if (allowedIds != null)
            {{
                if (predicate == null)
                    predicate = v => allowedIds.Contains(v.{vid});
                else
                {{
                    var oldPredicate = predicate;
                    predicate = v => allowedIds.Contains(v.{vid}) && oldPredicate.Compile()(v);
                }}
            }}
"""
        
        # The line to insert before is: var (items, count) = await _unitOfWork.
        # But for other files it might be just: var (items, count) = await
        import re
        content = re.sub(r"(\s+)(var \(items, count\) = await)", r"\1" + logic.strip() + r"\1\2", content)

    with open(path, "w") as f:
        f.write(content)
        print("Updated", path)
