using System.Security.Claims;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;
using DatabaseManagement.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatabaseManagement
{
    public class DatabaseManager<TContext, TEntity> : IDatabaseManager<TEntity, TContext>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> identityUserManager;

        public DatabaseManager(TContext context, IHttpContextAccessor httpContextAccessor,
            UserManager<User> identityUserManager)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.httpContextAccessor =
                httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.identityUserManager = identityUserManager;
        }

        public async Task<bool> AddAsync(TEntity entityToAdd)
        {
            if (!await AllowedToWrite(entityToAdd, true)) throw new UnauthorizedAccessException();
            
            entityToAdd = GenerateId(entityToAdd);

            await context.Set<TEntity>().AddAsync(entityToAdd);
            var result = await context.SaveChangesAsync();
            return result != 0;
        }

        public async Task<bool> DeleteAsync(TEntity objectToDelete, bool ignoreId = false)
        {
            var dbEntity = await GetByIdAsync((objectToDelete as dynamic).Id.ToString(), ignoreId);
            context.Set<TEntity>().Remove(dbEntity);
            var result = await context.SaveChangesAsync();
            return result != 0;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(bool ignoreId = false)
        {
            var query = context.Set<TEntity>().AsQueryable();

            if (typeof(TEntity) == typeof(User))
            {
                query = query.Include("Organization");
                query = query.Include("Teams");
            }
            
            var result = await query.ToListAsync();


            if (!result.Any())
                return result;

            if (ignoreId)
                return result;

            var user = await GetCurrentUserAsync();
            var isManager = await identityUserManager.IsInRoleAsync(user, Role.Management.ToString());

            var tasks = result.Select(async x => await AllowedToReadAsync(user, isManager, x));
            var allowedToReadResults = await Task.WhenAll(tasks);
            var filteredResult = result.Where((x, index) => allowedToReadResults[index]);

            return filteredResult;
        }

        public async Task<TEntity?> GetByIdAsync(string id, bool ignoreId = false)
        {
            var query = context.Set<TEntity>().AsQueryable();

            var result = query.AsEnumerable()
                .FirstOrDefault(x => x.GetType().GetProperty("Id")!.GetValue(x, null)!.Equals(id));

            if (ignoreId) return result;

            var user = await GetCurrentUserAsync();
            var isManager = await identityUserManager.IsInRoleAsync(user, Role.Management.ToString());
            var allowedToRead = await AllowedToReadAsync(user, isManager, result!);
            return allowedToRead ? result : null;
        }

        public async Task<bool> UpdateAsync(TEntity objectToUpdate, bool ignoreId = false)
        {
            var dbEntity = await GetByIdAsync((objectToUpdate as dynamic).Id.ToString(), ignoreId);
            var dbEntityProperties = context.Entry(dbEntity).OriginalValues.Properties;

            foreach (var property in dbEntityProperties)
            {
                if (property.Name == "CreatedDate" || property.Name == "CreatedBy" ||
                    property.Name == "UpdatedDate" || property.Name == "UpdatedBy" ||
                    property.Name == "Organization")
                    continue;

                var originalValue = context.Entry(dbEntity).OriginalValues[property];
                var newValue = context.Entry(objectToUpdate).CurrentValues[property];

                if (!object.Equals(originalValue, newValue))
                {
                    context.Entry(dbEntity).Property(property.Name).IsModified = true;
                    context.Entry(dbEntity).CurrentValues[property.Name] = newValue;
                }
            }

            if (!ignoreId && !await AllowedToWrite(objectToUpdate))
                throw new UnauthorizedAccessException();

            var result = await context.SaveChangesAsync();
            return result != 0;
        }

        private string GetUserId()
        {
            var userId = httpContextAccessor.HttpContext?.User.Claims.SingleOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return userId ?? throw new Exception("Error at getting user id");
        }

        private async Task<bool> AllowedToWrite(TEntity objectToEdit, bool addItem = false)
        {
            var currentUser = await GetCurrentUserAsync();

            //If nothing is modified
            // ReSharper disable once PossibleMultipleEnumeration
            if (objectToEdit == null)
            {
                throw new Exception("No changes were made.");
            }

            var entity = objectToEdit as AuditableEntity;

            if (entity == null)
                throw new Exception("Error ad reading auditable entity");

            if (addItem)
            {
                entity.CreatedBy = (currentUser.Id == null ? "-1" : currentUser.Id) ??
                                   throw new InvalidOperationException();
                entity.Organization = (currentUser.Id == null ? "-1" : currentUser.Organization.Id) ??
                                      throw new InvalidOperationException();
                entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entity.Organization != "-1")
            {
                if (entity.Organization != currentUser.Organization.Id)
                    return false;
            }

            entity.UpdatedBy = (currentUser.Id == null ? "-1" : currentUser.Id) ??
                               throw new InvalidOperationException();
            entity.UpdatedDate = DateTime.UtcNow;
            return true;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var dbEmployees = identityUserManager.Users
                .Include(u => u.Organization);
            var currentUserId = GetUserId();
            var user = await dbEmployees.SingleAsync(e => e.Id == currentUserId);
            return user;
        }

        private async Task<bool> AllowedToReadAsync(User user, bool isManager, TEntity objectToRead)
        {
            if (!isManager)
            {
                return objectToRead.GetType().GetProperty("CreatedBy")!
                    .GetValue(objectToRead, null)!.Equals(user.Id);
            }

            return objectToRead.GetType().GetProperty("Organization")!
                .GetValue(objectToRead, null)!.Equals(user.Organization.Id);
        }
        
        private TEntity GenerateId(TEntity entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString()))
                {
                    entity.GetType().GetProperty("Id")?.SetValue(entity, Guid.NewGuid().ToString());
                    return entity;
                }

                return entity;
            }
            catch
            {
                //ignore
                return entity;
            }
        }
    }
}