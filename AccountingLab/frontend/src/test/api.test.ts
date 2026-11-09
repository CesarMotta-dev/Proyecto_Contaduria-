import { describe, expect, it, vi } from 'vitest';
import { ApiError, isDecimal, money, request } from '../services/api';

describe('Transporte y representación de importes', () => {
  it('conserva cifras monetarias superiores al entero seguro sin redondearlas', () => {
    expect(money('999999999999999.1234', 'COP')).toBe('999.999.999.999.999,1234 COP');
    expect(money('-0.0001')).toBe('−0,0001');
    expect(money('10')).toBe('10,00');
    expect(money(10)).toBe('—');
  });
  it('valida el formato decimal sin aceptar exponentes, comas ni más de cuatro decimales', () => {
    expect(isDecimal('100.0001')).toBe(true);
    for (const value of ['0.00001', '1e3', '1,23', '-1', 'NaN', '']) expect(isDecimal(value)).toBe(false);
    expect(isDecimal('0.123456', 6)).toBe(true);
  });
  it('envía token y empresa autorizada con importes textuales intactos', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ id: 'x' }), { status: 201 }));
    vi.stubGlobal('fetch', fetchMock);
    await request('/payments', 'token-demo', 'company-2', { method: 'POST', body: JSON.stringify({ amount: '0.1000' }) });
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/payments');
    expect(options.headers.get('Authorization')).toBe('Bearer token-demo');
    expect(options.headers.get('X-Company-Id')).toBe('company-2');
    expect(options.headers.get('Content-Type')).toBe('application/json');
    expect(JSON.parse(options.body).amount).toBe('0.1000');
  });
  it('conserva status y datos de validación sin confundirlos con un patrón pendiente', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ detail: 'Periodo cerrado', errors: { date: ['El periodo no admite asientos.'] } }), { status: 409 })));
    await expect(request('/journal-entries', 'token', 'company')).rejects.toMatchObject({ status: 409, message: 'Periodo cerrado', problem: { errors: { date: ['El periodo no admite asientos.'] } } });
  });
  it('distingue falta de conexión de un fallo académico', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));
    try { await request('/accounts'); throw new Error('La solicitud debía fallar.'); } catch (error) { expect(error).toBeInstanceOf(ApiError); expect(error).toMatchObject({ status: 0 }); }
  });
  it('conserva el aborto de consulta para descartar respuestas de otra empresa', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new DOMException('Aborted', 'AbortError')));
    await expect(request('/accounts')).rejects.toMatchObject({ name: 'AbortError' });
  });
});
