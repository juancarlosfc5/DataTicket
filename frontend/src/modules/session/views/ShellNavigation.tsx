import { InternalLink } from '../../../shared/ui/InternalLink'
import type { NavigationItem, SectionId } from '../models/navigation'

interface ShellNavigationProps {
  readonly label: string
  readonly items: readonly NavigationItem[]
  readonly currentItemId: SectionId | null
  readonly onNavigate: (to: string) => void
}

/** Navegación del shell; la sección actual lleva `aria-current="page"`. */
export function ShellNavigation({ label, items, currentItemId, onNavigate }: ShellNavigationProps) {
  if (items.length === 0) return null
  return (
    <nav className="shell-nav" aria-label={label}>
      <ul className="shell-nav__list">
        {items.map((item) => (
          <li key={item.id}>
            <InternalLink
              className="shell-nav__link"
              to={item.to}
              onNavigate={onNavigate}
              aria-current={item.id === currentItemId ? 'page' : undefined}
            >
              {item.label}
            </InternalLink>
          </li>
        ))}
      </ul>
    </nav>
  )
}
