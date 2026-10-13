import type { HTMLAttributes, Ref } from 'react'

interface LoginFieldProps {
  readonly ref: Ref<HTMLInputElement>
  readonly id: string
  readonly label: string
  readonly type: 'email' | 'password'
  readonly autoComplete: string
  readonly inputMode?: HTMLAttributes<HTMLInputElement>['inputMode']
  readonly value: string
  readonly error: string | undefined
  readonly onChange: (value: string) => void
}

/**
 * Campo del formulario de entrada: etiqueta, control y error asociado por `aria-describedby`.
 * Entre la etiqueta y el control no cabe nada enfocable: rompería el orden Correo → Contraseña.
 */
export function LoginField({ ref, id, label, type, autoComplete, inputMode, value, error, onChange }: LoginFieldProps) {
  const errorId = `${id}-error`
  return (
    <div className={`signin-field${error ? ' signin-field--invalid' : ''}`}>
      <label className="signin-field__label" htmlFor={id}>
        {label}
      </label>
      <input
        ref={ref}
        id={id}
        name={id}
        className="signin-field__input"
        type={type}
        autoComplete={autoComplete}
        inputMode={inputMode}
        autoCapitalize="none"
        autoCorrect="off"
        spellCheck={false}
        value={value}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? errorId : undefined}
        onChange={(event) => onChange(event.target.value)}
      />
      {error ? (
        <p id={errorId} className="signin-field__error">
          {error}
        </p>
      ) : null}
    </div>
  )
}
