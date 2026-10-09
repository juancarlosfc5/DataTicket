interface ShellUserProps {
  readonly name: string
  readonly detail: string
  readonly initials: string
  readonly onSignOut: () => void
  readonly isSigningOut: boolean
}

/** Usuario de la sesión y acción «Cerrar sesión» (deshabilitada mientras se cierra). */
export function ShellUser({ name, detail, initials, onSignOut, isSigningOut }: ShellUserProps) {
  return (
    <div className="shell-user">
      <span className="avatar" aria-hidden="true">
        {initials}
      </span>
      <div className="shell-user__text">
        <span className="shell-user__name">{name}</span>
        <span className="shell-user__detail">{detail}</span>
      </div>
      <button type="button" className="btn btn--quiet" onClick={onSignOut} disabled={isSigningOut}>
        {isSigningOut ? 'Cerrando sesión…' : 'Cerrar sesión'}
      </button>
    </div>
  )
}
