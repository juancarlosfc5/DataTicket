import { describe, expect, it } from 'vitest'
import { toHealthSummary } from './health'

describe('toHealthSummary', () => {
  it('marca el sistema como operativo cuando el backend reporta Healthy', () => {
    const summary = toHealthSummary({
      status: 'Healthy',
      checks: [{ name: 'postgresql', status: 'Healthy' }],
    })

    expect(summary).toEqual({
      status: 'Healthy',
      isOperational: true,
      dependencies: [{ name: 'postgresql', status: 'Healthy' }],
    })
  })

  it('trata estados desconocidos como Unhealthy', () => {
    const summary = toHealthSummary({ status: 'Maybe', checks: [{ name: 'postgresql', status: 42 }] })

    expect(summary.status).toBe('Unhealthy')
    expect(summary.isOperational).toBe(false)
    expect(summary.dependencies).toEqual([{ name: 'postgresql', status: 'Unhealthy' }])
  })

  it('descarta dependencias mal formadas y tolera checks ausentes', () => {
    expect(toHealthSummary({ status: 'Degraded', checks: [null, { status: 'Healthy' }] }).dependencies).toEqual([])
    expect(toHealthSummary({ status: 'Degraded' }).dependencies).toEqual([])
  })
})
