import re

path = "src/Filo.Application/Features/Person/Queries/GetPagedPersonQuery.cs"
with open(path, "r") as f:
    content = f.read()

if "IRbacService" not in content:
    if "using Filo.Application.Common.Interfaces;" not in content:
        content = "using Filo.Application.Common.Interfaces;\n" + content
    
    # 1. Add fields
    content = content.replace("private readonly ICacheService _cacheService;", "private readonly ICacheService _cacheService;\n    private readonly IRbacService _rbacService;")
    
    # 2. Add constructor argument
    content = content.replace("public GetPagedPersonQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)", "public GetPagedPersonQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IRbacService rbacService)")
    
    # 3. Add constructor assignment
    content = content.replace("_cacheService = cacheService;", "_cacheService = cacheService;\n        _rbacService = rbacService;")
    
    # 4. Inject filtering logic
    logic = """
            var allowedPersonIds = await _rbacService.GetAllowedPersonIdsAsync();
            if (allowedPersonIds != null)
            {
                if (predicate == null)
                    predicate = p => allowedPersonIds.Contains(p.Id);
                else
                {
                    var oldPredicate = predicate;
                    predicate = p => allowedPersonIds.Contains(p.Id) && oldPredicate.Compile()(p);
                }
            }
"""
    content = re.sub(r"(\s+)(var \(items, count\) = await _unitOfWork\.Person\.GetPagedAsync)", r"\1" + logic.strip() + r"\1\2", content)

with open(path, "w") as f:
    f.write(content)
    print("Fixed Person Query RBAC.")
