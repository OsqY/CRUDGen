namespace CRUDGen;

public static class Consts
{
    public static string GetGenericRepositoryString(string repoNamespace,string dbContextNameSpace)
    {
        return $@"
using Microsoft.EntityFrameworkCore;

namespace {repoNamespace};
public interface IRepository<TEntity> where TEntity : class 
{{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}}
public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
{{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext context)
    {{
        _context = context;
        _dbSet = context.Set<TEntity>();
    }}

    public async Task<TEntity?> GetByIdAsync(int id)
    {{
        return await _dbSet.FindAsync(id);
    }}

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {{
        return await _dbSet.ToListAsync();
    }}

    public async Task AddAsync(TEntity entity)
    {{
        await _dbSet.AddAsync(entity);
    }}

    public void Update(TEntity entity)
    {{
        _dbSet.Update(entity);
    }}

    public void Delete(TEntity entity)
    {{
        _dbSet.Remove(entity);
    }}
}}";
    }

    public static string RepositoryStringImplementation(string modelName, string modelNamespace,
        string interfacesNamespace)
    {
        return $@"
using {modelNamespace};
using Microsoft.EntityFrameworkCore;

namespace {interfacesNamespace};

public interface I{modelName}Repository : IRepository<{modelName}>
{{
}}

public class {modelName}Repository(DbContext context)
    : GenericRepository<{modelName}>(context), I{modelName}Repository
{{
}}
";
    }

    public static string GetGenericService()
    {
        return @"
public interface IService<TEntity, TCDto, TUDto> where TEntity : class 
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(int id);
    Task<int> CreateAsync(TCDto dto);
    Task<bool> UpdateAsync(int id, TUDto dto);
    Task<bool> DeleteAsync(int id);
}
";
    }

    public static string GetServiceString(string modelName, string dtoNamespace, 
        string modelNamespace, string serviceNamespace, string interfacesNamespace)
    {
        return $@"
using AutoMapper;
using {modelNamespace};
using {dtoNamespace};
using {interfacesNamespace};

namespace {serviceNamespace};

public interface I{modelName}Service : IService<{modelName}, Create{modelName}Dto, Update{modelName}Dto>
{{
}}

public class {modelName}Service(I{modelName}Repository {modelName.ToLower()}Repository, IMapper mapper) : I{modelName}Service 
{{
   public async Task<IEnumerable<ProductCategory>> GetAllAsync() 
   {{
        return await {modelName.ToLower()}Repository.GetAllAsync();
   }} 

    public async Task<{modelName}?> GetByIdAsync(int id)
    {{
        return await {modelName.ToLower()}Repository.GetByIdAsync(id);
    }}

    public async Task<int> CreateAsync(Create{modelName}Dto create{modelName}Dto) 
    {{
        var {modelName.ToLower()} = mapper.Map<{modelName}>(create{modelName}Dto);
        await {modelName.ToLower()}Repository.AddAsync({modelName.ToLower()});
        return {modelName.ToLower()}.Id;
    }}

    public async Task<bool> UpdateAsync(int id, Update{modelName}Dto update{modelName}Dto)
    {{
        var {modelName.ToLower()} = await {modelName.ToLower()}Repository.GetByIdAsync(id);
        
        if ({modelName.ToLower()} != null) 
        {{
            {modelName.ToLower()} = mapper.Map<{modelName}>(update{modelName}Dto);
            {modelName.ToLower()}Repository.Update({modelName.ToLower()});
            return true;
        }}
        return false;
    }}

    public async Task<bool> DeleteAsync(int id)
    {{
        var {modelName.ToLower()} = await {modelName.ToLower()}Repository.GetByIdAsync(id);
        
        if ({modelName.ToLower()} != null)
        {{ 
            {modelName.ToLower()}Repository.Delete({modelName.ToLower()});
            return true;
        }}
        return false;
    }}
}}
";
    }

    public static string GetControllerString(string modelsNamespace, string interfacesNamespace,
        string dtosNamespaces, string controllersNamespace, string modelName)
    {
        return $@"
using {modelsNamespace};
using {interfacesNamespace};
using {dtosNamespaces};
using Microsoft.AspNetCore.Mvc;

namespace {controllersNamespace};

[ApiController]
[Route(""api/[controller]"")]
public class {modelName}Controller(I{modelName}Service {modelName.ToLower()}Service) : ControllerBase
{{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {{
        return Ok(await {modelName.ToLower()}Service.GetAllAsync();
    }}
    
    [HttpGet(""{{id}}""]
    public async Task<IActionResult> GetById(int id)
    {{
        var result = await {modelName.ToLower()}Service.Get{modelName}Async(id);
        
        return result != null ? Ok(result) : NotFound();
    }}

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Create{modelName}Dto create{modelName}Dto)
    {{
        var id = await {modelName.ToLower()}Service.Create{modelName}Async(create{modelName}Dto);
        return CreatedAtAction(nameof({modelName}), id);
    }}
    
    [HttpPut(""{{id}}"")]
    public async Task<IActionResult> Put(int id, [FromBody] Update{modelName}Dto update{modelName}Dto)
    {{
        await {modelName.ToLower()}Service.Update{modelName}Async(id, update{modelName}Dto);
        return NoContent();
    }}
    
    [HttpDelete(""{{id}}"")]
    public async Task<IActionResult> Delete(int id)
    {{
        await {modelName.ToLower()}Service.Delete{modelName}Async(id);
        return NoContent();
    }}
}}    
";
    }
}