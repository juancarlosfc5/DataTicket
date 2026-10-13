using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// <see cref="UserManager{TUser}"/> cuyo bloqueo usa el <see cref="TimeProvider"/> de la aplicación.
/// En ASP.NET Core 10 <c>IsLockedOutAsync</c> y <c>AccessFailedAsync</c> usan <c>DateTimeOffset.UtcNow</c>
/// fijo; aquí se reproducen con el mismo comportamiento pero con el reloj inyectado (decisión de B3), para
/// que el bloqueo se pruebe con <c>FakeTimeProvider</c> (HU-003 CHU-05) y no haya un reloj paralelo.
/// </summary>
public sealed class DataTicketUserManager(
    IUserStore<ApplicationUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IEnumerable<IUserValidator<ApplicationUser>> userValidators,
    IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<ApplicationUser>> logger,
    TimeProvider timeProvider)
    : UserManager<ApplicationUser>(
        store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
{
    public override async Task<bool> IsLockedOutAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var lockoutStore = GetLockoutStore();

        if (!await lockoutStore.GetLockoutEnabledAsync(user, CancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var lockoutEnd = await lockoutStore.GetLockoutEndDateAsync(user, CancellationToken).ConfigureAwait(false);
        return lockoutEnd >= timeProvider.GetUtcNow();
    }

    public override async Task<IdentityResult> AccessFailedAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var lockoutStore = GetLockoutStore();

        // Si este fallo alcanza el umbral, se bloquea y se reinicia el contador (igual que Identity).
        var count = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken).ConfigureAwait(false);
        if (count < Options.Lockout.MaxFailedAccessAttempts)
        {
            return await UpdateUserAsync(user).ConfigureAwait(false);
        }

        Logger.LogInformation("Cuenta {UserId} bloqueada tras {Count} intentos fallidos.", user.Id, count);
        var lockoutEnd = timeProvider.GetUtcNow().Add(Options.Lockout.DefaultLockoutTimeSpan);
        await lockoutStore.SetLockoutEndDateAsync(user, lockoutEnd, CancellationToken).ConfigureAwait(false);
        await lockoutStore.ResetAccessFailedCountAsync(user, CancellationToken).ConfigureAwait(false);
        return await UpdateUserAsync(user).ConfigureAwait(false);
    }

    private IUserLockoutStore<ApplicationUser> GetLockoutStore() =>
        Store as IUserLockoutStore<ApplicationUser>
        ?? throw new NotSupportedException("El almacén de usuarios no admite bloqueo.");
}
