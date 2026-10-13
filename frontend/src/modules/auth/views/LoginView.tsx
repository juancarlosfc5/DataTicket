import { useEffect, useRef, type FormEvent } from 'react'
import { BrandMark } from '../../../shared/ui/BrandMark'
import { InternalLink } from '../../../shared/ui/InternalLink'
import type { LoginState } from '../models/loginState'
import { LoginField } from './LoginField'
import './LoginView.css'

export interface LoginViewProps {
  readonly state: LoginState
  readonly onEmailChange: (value: string) => void
  readonly onPasswordChange: (value: string) => void
  readonly onSubmit: () => void
  readonly onNavigate: (to: string) => void
}

const FORGOT_PASSWORD_PATH = '/olvide-mi-contrasena'

/**
 * Entrada a DataTicket (HU-003): un único formulario de correo y contraseña.
 * Vista pura: el foco tras un fallo sigue la petición `state.focus` del controlador.
 */
export function LoginView({ state, onEmailChange, onPasswordChange, onSubmit, onNavigate }: LoginViewProps) {
  const emailRef = useRef<HTMLInputElement>(null)
  const passwordRef = useRef<HTMLInputElement>(null)
  const alertRef = useRef<HTMLDivElement>(null)
  const { status, focus } = state
  const isSubmitting = status.kind === 'submitting'
  const fieldErrors = status.kind === 'error' ? status.fieldErrors : {}
  const message = status.kind === 'error' ? status.message : null

  useEffect(() => {
    if (!focus) return
    const targets = { email: emailRef, password: passwordRef, alert: alertRef }
    targets[focus.target].current?.focus()
  }, [focus])

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!isSubmitting) onSubmit()
  }

  return (
    <main className="signin theme-light">
      <div className="signin__frame">
        <header className="signin__top">
          <BrandMark />
          <span className="signin__brand">DataTicket</span>
          <span className="signin__org">Data Global</span>
        </header>

        <div className="signin__grid">
          <section className="signin__intro" aria-labelledby="signin-title">
            <h1 id="signin-title" className="signin__title">
              Ingresa a <span className="signin__title-accent">DataTicket</span>
            </h1>
            <p className="signin__lead">
              El portal de soporte de Data Global: radica y sigue tus solicitudes, o atiéndelas si eres parte del
              equipo.
            </p>
          </section>

          {/* method="post": defensa en profundidad; si el JS fallara, la contraseña nunca va en la URL. */}
          <form
            className="signin__card"
            method="post"
            noValidate
            aria-labelledby="signin-form-title"
            onSubmit={handleSubmit}
          >
            <h2 id="signin-form-title" className="signin__card-title">
              Iniciar sesión
            </h2>

            {message ? (
              <div key={state.attempt} ref={alertRef} className="signin__alert" role="alert" tabIndex={-1}>
                {message}
              </div>
            ) : null}

            <LoginField
              ref={emailRef}
              id="signin-email"
              label="Correo"
              type="email"
              autoComplete="username"
              inputMode="email"
              value={state.email}
              error={fieldErrors.email}
              onChange={onEmailChange}
            />

            <LoginField
              ref={passwordRef}
              id="signin-password"
              label="Contraseña"
              type="password"
              autoComplete="current-password"
              value={state.password}
              error={fieldErrors.password}
              onChange={onPasswordChange}
            />

            <button type="submit" className="btn btn--primary signin__submit" disabled={isSubmitting}>
              {isSubmitting ? 'Ingresando…' : 'Ingresar'}
            </button>

            {/* Tras la acción principal, en el DOM y a la vista: Tab va de Correo a Contraseña y a Ingresar. */}
            <p className="signin__help">
              <InternalLink className="signin__link" to={FORGOT_PASSWORD_PATH} onNavigate={onNavigate}>
                ¿Olvidaste tu contraseña?
              </InternalLink>
            </p>
          </form>
        </div>

        <footer className="signin__foot">Data Global S.A.S. · Soporte</footer>
      </div>
    </main>
  )
}
