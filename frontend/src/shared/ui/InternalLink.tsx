import type { AnchorHTMLAttributes, MouseEvent } from 'react'

export interface InternalLinkProps extends Omit<AnchorHTMLAttributes<HTMLAnchorElement>, 'href' | 'onClick'> {
  readonly to: string
  readonly onNavigate: (to: string) => void
}

function isPlainLeftClick(event: MouseEvent<HTMLAnchorElement>): boolean {
  return event.button === 0 && !event.metaKey && !event.altKey && !event.ctrlKey && !event.shiftKey
}

/**
 * Enlace interno sin dependencia del enrutador: es un `<a href>` real (abrir en otra pestaña,
 * copiar enlace) y, con un clic simple, delega la navegación en `onNavigate`.
 */
export function InternalLink({ to, onNavigate, children, ...anchorProps }: InternalLinkProps) {
  const handleClick = (event: MouseEvent<HTMLAnchorElement>) => {
    if (event.defaultPrevented || !isPlainLeftClick(event)) return
    event.preventDefault()
    onNavigate(to)
  }

  return (
    <a {...anchorProps} href={to} onClick={handleClick}>
      {children}
    </a>
  )
}
