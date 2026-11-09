import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { App } from '../App';
import { ResourcePage } from '../pages/ResourcePage';
import { JournalPage } from '../pages/JournalPage';
import { ChallengesPage } from '../pages/ChallengesPage';
import { ExportsPage } from '../pages/ReportsPage';
import { LoginPage } from '../pages/LoginPage';
import { ChallengeAction } from '../components/ChallengeAction';
import { modules } from '../services/modules';
import type { Session } from '../types/api';

const context = { token: 'token-test', companyId: 'company-1', currency: 'COP', role: 'Administrador' };
const session: Session = { accessToken: 'token-test', user: { id: 'user-1', name: 'Docente', email: 'docente@example.test' }, companies: [{ id: 'company-1', name: 'Empresa Uno', currency: 'COP', role: 'Administrador' }, { id: 'company-2', name: 'Empresa Dos', currency: 'USD', role: 'Consulta' }] };
const envelope = (items: unknown[] = [], total = items.length) => ({ items, total, page: 1, pageSize: 10 });
const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
const accountModule = modules.find(module => module.path === 'accounts')!;
const pending = { title: 'Funcionalidad pendiente', status: 501, detail: 'Este desafío requiere una implementación del estudiante.', challengeId: 'DP-08', patternName: 'Composite', referenceUrl: 'https://refactoring.guru/es/design-patterns/composite' };

function setupFetch(handler?: (path: string, options: RequestInit) => Response | Promise<Response>) {
  const mock = vi.fn(async (input: string | URL | Request, options: RequestInit) => handler ? handler(String(input), options) : json(envelope()));
  vi.stubGlobal('fetch', mock); return mock;
}
function app(path: string) { sessionStorage.setItem('accountinglab-session', JSON.stringify(session)); return render(<MemoryRouter initialEntries={[path]}><App/></MemoryRouter>); }

describe('Acceso y navegación', () => {
  it('muestra autenticación real sin datos contables de respaldo', () => {
    setupFetch(); render(<MemoryRouter><App/></MemoryRouter>);
    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument();
    expect(screen.getByLabelText('Contraseña')).toHaveValue('');
    expect(screen.queryByRole('heading', { name: 'Últimos asientos' })).not.toBeInTheDocument();
  });
  it('conserva credenciales para corregir una respuesta 401 y no crea una sesión', async () => {
    const login = vi.fn(); setupFetch(() => json({ detail: 'Credenciales inválidas.' }, 401));
    render(<LoginPage onLogin={login}/>); const user = userEvent.setup();
    await user.type(screen.getByLabelText('Correo electrónico'), 'persona@example.test'); await user.type(screen.getByLabelText('Contraseña'), 'invalida'); await user.click(screen.getByRole('button', { name: /Entrar a AccountingLab/ }));
    expect(await screen.findByText('Credenciales inválidas.')).toBeInTheDocument(); expect(login).not.toHaveBeenCalled(); expect(screen.getByLabelText('Correo electrónico')).toHaveValue('persona@example.test');
  });
  it('envía las credenciales a la API y utiliza las empresas retornadas', async () => {
    const login = vi.fn(); const fetchMock = setupFetch(() => json(session)); render(<LoginPage onLogin={login}/>); const user = userEvent.setup();
    await user.type(screen.getByLabelText('Correo electrónico'), 'docente@example.test'); await user.type(screen.getByLabelText('Contraseña'), 'clave-de-prueba'); await user.click(screen.getByRole('button', { name: /Entrar a AccountingLab/ }));
    await waitFor(() => expect(login).toHaveBeenCalledWith(session)); expect(JSON.parse(String(fetchMock.mock.calls[0][1].body))).toEqual({ email: 'docente@example.test', password: 'clave-de-prueba' });
  });
  it('ofrece acceso a los veinte módulos básicos y el catálogo académico', async () => {
    setupFetch(); app('/accounts'); await screen.findByRole('heading', { name: 'Cuentas contables' });
    for (const href of ['/companies', '/users', '/accounts', '/journal-entries', '/journal', '/ledger', '/third-parties', '/receivables', '/payables', '/invoices', '/tax-rules', '/bank-reconciliation', '/cost-centers', '/budgets', '/reports', '/audit', '/notifications', '/exports', '/approvals', '/periods', '/challenges']) {
      expect(document.querySelector(`nav a[href="${href}"]`)).toBeInTheDocument();
    }
  });
  it('cambia de empresa, limpia borradores y utiliza el nuevo encabezado', async () => {
    const fetchMock = setupFetch((_path, options) => json(envelope([{ id: 'account', code: new Headers(options.headers).get('X-Company-Id'), name: 'Caja', isActive: true }])));
    app('/accounts'); await screen.findByText('company-1'); await userEvent.click(screen.getByRole('button', { name: 'Nuevo registro' })); await userEvent.type(screen.getByLabelText('Nombre *'), 'Borrador empresa uno');
    await userEvent.selectOptions(screen.getByLabelText('Empresa activa'), 'company-2');
    expect(await screen.findByText('company-2')).toBeInTheDocument(); expect(screen.queryByDisplayValue('Borrador empresa uno')).not.toBeInTheDocument();
    expect(fetchMock.mock.calls.some(([, options]) => new Headers(options.headers).get('X-Company-Id') === 'company-2')).toBe(true); expect(screen.queryByRole('button', { name: 'Nuevo registro' })).not.toBeInTheDocument();
  });
  it('elimina el token y la empresa al cerrar sesión', async () => {
    setupFetch(); app('/accounts'); await userEvent.click(screen.getByRole('button', { name: 'Cerrar sesión' }));
    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument(); expect(sessionStorage.getItem('accountinglab-session')).toBeNull(); expect(sessionStorage.getItem('accountinglab-company')).toBeNull();
  });
  it('actualiza las empresas autorizadas sin reemplazar el token', async () => {
    setupFetch(path => json(path.includes('/companies?') ? envelope([...session.companies, { id: 'company-new', name: 'Nueva empresa', currency: 'EUR', role: 'Administrador' }]) : envelope()));
    app('/accounts'); await userEvent.click(screen.getByRole('button', { name: 'Actualizar empresas' }));
    expect(await screen.findByRole('option', { name: 'Nueva empresa · EUR' })).toBeInTheDocument();
    expect(JSON.parse(sessionStorage.getItem('accountinglab-session')!).accessToken).toBe('token-test');
  });
});

describe('Formularios conectados y errores', () => {
  it('guarda una cuenta mediante POST y consulta la lista confirmada', async () => {
    let saved = false; const fetchMock = setupFetch((_path, options) => { if (options.method === 'POST') { saved = true; return json({ id: 'saved-account' }, 201); } return json(envelope(saved ? [{ id: 'saved-account', code: '1105', name: 'Caja persistida', isGroup: false, isActive: true }] : [])); });
    render(<ResourcePage module={accountModule} {...context}/>); const user = userEvent.setup(); await user.click(screen.getByRole('button', { name: 'Nuevo registro' }));
    await user.type(screen.getByLabelText('Código *'), '1105'); await user.type(screen.getByLabelText('Nombre *'), 'Caja persistida'); await user.click(screen.getByRole('button', { name: 'Guardar registro' }));
    expect(await screen.findByText('Caja persistida')).toBeInTheDocument(); expect(screen.getByRole('status')).toHaveTextContent('Registro guardado');
    const posted = fetchMock.mock.calls.find(([, options]) => options.method === 'POST')!; expect(JSON.parse(String(posted[1].body))).toEqual({ code: '1105', name: 'Caja persistida', isGroup: false, parentId: null, isActive: true });
  });
  it('muestra conflicto del servidor, mantiene la entrada y evita un éxito falso', async () => {
    setupFetch((_path, options) => options.method === 'POST' ? json({ detail: 'El código ya existe.' }, 409) : json(envelope()));
    render(<ResourcePage module={accountModule} {...context}/>); const user = userEvent.setup(); await user.click(screen.getByRole('button', { name: 'Nuevo registro' })); await user.type(screen.getByLabelText('Código *'), '1105'); await user.type(screen.getByLabelText('Nombre *'), 'Caja'); await user.click(screen.getByRole('button', { name: 'Guardar registro' }));
    expect(await screen.findByText('El código ya existe.')).toBeInTheDocument(); expect(screen.getByLabelText('Nombre *')).toHaveValue('Caja'); expect(screen.queryByText(/Registro guardado/)).not.toBeInTheDocument();
  });
  it('rechaza un importe de cinco decimales sin enviar la operación', async () => {
    const fetchMock = setupFetch(path => json(envelope(path.includes('third-parties') ? [{ id: 'third-1', name: 'Cliente' }] : [])));
    render(<ResourcePage module={modules.find(m => m.path === 'receivables')!} {...context}/>); const user = userEvent.setup(); await user.click(screen.getByRole('button', { name: 'Nuevo registro' }));
    await user.selectOptions(screen.getByLabelText('Tercero *'), await screen.findByRole('option', { name: 'Cliente' })); await user.type(screen.getByLabelText('Descripción *'), 'Servicio'); await user.type(screen.getByLabelText('Importe *'), '0.12345'); fireEvent.change(screen.getByLabelText('Vencimiento *'), { target: { value: '2026-11-01' } }); await user.click(screen.getByRole('button', { name: 'Guardar registro' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('máximo 4 decimales'); expect(fetchMock.mock.calls.filter(([, options]) => options.method === 'POST')).toHaveLength(0);
  });
  it('conserva el importe textual al registrar una obligación', async () => {
    const fetchMock = setupFetch((path, options) => options.method === 'POST' ? json({ id: 'r1' }, 201) : json(envelope(path.includes('third-parties') ? [{ id: 'third-1', name: 'Cliente' }] : [])));
    render(<ResourcePage module={modules.find(m => m.path === 'receivables')!} {...context}/>); const user = userEvent.setup(); await user.click(screen.getByRole('button', { name: 'Nuevo registro' })); await user.selectOptions(screen.getByLabelText('Tercero *'), await screen.findByRole('option', { name: 'Cliente' })); await user.type(screen.getByLabelText('Descripción *'), 'Servicio'); await user.type(screen.getByLabelText('Importe *'), '999999999999999.1000'); fireEvent.change(screen.getByLabelText('Vencimiento *'), { target: { value: '2026-11-01' } }); await user.click(screen.getByRole('button', { name: 'Guardar registro' }));
    await waitFor(() => expect(fetchMock.mock.calls.some(([, options]) => options.method === 'POST')).toBe(true)); const posted = fetchMock.mock.calls.find(([, options]) => options.method === 'POST')!; expect(JSON.parse(String(posted[1].body)).amount).toBe('999999999999999.1000');
  });
  it('identifica falta de conexión sin mostrar registros simulados', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline'))); render(<ResourcePage module={accountModule} {...context}/>);
    expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo conectar con la API'); expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
  it('respeta la página de datos y permite filtrar sin aparentar búsqueda global', async () => {
    const fetchMock = setupFetch(path => json(envelope([{ id: 'one', code: '1105', name: path.includes('page=2') ? 'Bancos' : 'Caja' }], 11)));
    render(<ResourcePage module={accountModule} {...context}/>); await screen.findByText('Caja'); await userEvent.click(screen.getByRole('button', { name: 'Siguiente' })); expect(await screen.findByText('Bancos')).toBeInTheDocument(); expect(fetchMock.mock.calls.some(([path]) => String(path).includes('page=2'))).toBe(true);
    await userEvent.type(screen.getByLabelText('Filtrar registros de esta página'), 'Sin coincidencia'); expect(screen.queryByText('Bancos')).not.toBeInTheDocument();
  });
});

describe('Desafíos y evidencia académica', () => {
  it('presenta un 501 con patrón y referencia, sin afirmar que se confirmó la operación', async () => {
    setupFetch(() => json(pending, 501)); render(<ChallengeAction ids={['DP-08']} token={context.token} companyId={context.companyId}/>); await userEvent.click(screen.getByRole('button', { name: 'DP-08' }));
    const alert = await screen.findByRole('alert'); expect(alert).toHaveTextContent('Funcionalidad no disponible'); expect(alert).toHaveTextContent('DP-08 · Composite'); expect(alert).toHaveTextContent('La operación contable no se ha confirmado.'); expect(screen.getByRole('link', { name: /Refactoring.Guru/ })).toHaveAttribute('href', pending.referenceUrl); expect(screen.queryByRole('status')).not.toBeInTheDocument();
  });
  it('muestra los 22 IDs proporcionados por la API y separa la autodeclaración de pruebas', async () => {
    const fixture = Array.from({ length: 22 }, (_, index) => ({ id: `DP-${String(index + 1).padStart(2, '0')}`, name: `Desafío ${index + 1}`, family: index < 5 ? 'Creacional' : index < 12 ? 'Estructural' : 'Comportamiento', context: 'Problema académico de la empresa.', referenceUrl: 'https://refactoring.guru/es/design-patterns/catalog', available: false }));
    setupFetch(() => json(fixture)); render(<ChallengesPage {...context}/>); expect(await screen.findByText('22 desafíos')).toBeInTheDocument();
    for (let id = 1; id <= 22; id++) expect(screen.getByRole('button', { name: `DP-${String(id).padStart(2, '0')}` })).toBeInTheDocument();
    await userEvent.selectOptions(screen.getByLabelText('Mi avance personal · autodeclarado', { selector: '#progress-DP-01' }), 'Autodeclaro terminado'); expect(screen.getAllByText('Pruebas: sin verificar')).toHaveLength(22); expect(screen.queryByText('Aprobado')).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Comportamiento' })); expect(screen.getAllByText('Pruebas: sin verificar')).toHaveLength(10);
  });
  it('conserva un 403 como falta de permiso y no como funcionalidad pendiente', async () => {
    setupFetch(() => json({ detail: 'No tienes permiso para consultar este informe.' }, 403)); render(<ChallengeAction ids={['DP-12']} token={context.token} companyId={context.companyId}/>); await userEvent.click(screen.getByRole('button', { name: 'DP-12' })); expect(await screen.findByRole('alert')).toHaveTextContent('Error 403'); expect(screen.queryByText('Funcionalidad no disponible')).not.toBeInTheDocument();
  });
  it('no representa una exportación denegada como archivo recibido', async () => {
    setupFetch(() => json({ detail: 'Exportación no autorizada.' }, 403)); render(<ExportsPage {...context}/>); await userEvent.click(screen.getAllByRole('button', { name: 'Descargar CSV' })[0]); expect(await screen.findByText('Exportación no autorizada.')).toBeInTheDocument(); expect(screen.queryByText('Archivo recibido de la API.')).not.toBeInTheDocument();
  });
});

describe('Asientos manuales', () => {
  it('envía líneas exactas y conserva el identificador al reintentar un fallo', async () => {
    const fetchMock = setupFetch((path, options) => options.method === 'POST' ? json({ detail: 'No hay periodo abierto.' }, 409) : json(envelope(path.includes('/accounts?') ? [{ id: 'cash', code: '1105', name: 'Caja', isActive: true, isGroup: false }, { id: 'capital', code: '3105', name: 'Capital', isActive: true, isGroup: false }] : [])));
    render(<JournalPage {...context}/>); const user = userEvent.setup(); await user.click(screen.getByRole('button', { name: 'Nuevo asiento' })); await user.type(screen.getByLabelText('Referencia *'), 'INICIAL'); fireEvent.change(screen.getByLabelText('Fecha contable *'), { target: { value: '2026-10-23' } });
    await user.selectOptions(screen.getByLabelText('Cuenta línea 1'), 'cash'); await user.selectOptions(screen.getByLabelText('Cuenta línea 2'), 'capital'); await user.clear(screen.getByLabelText('Débito línea 1')); await user.type(screen.getByLabelText('Débito línea 1'), '0.1000'); await user.clear(screen.getByLabelText('Crédito línea 2')); await user.type(screen.getByLabelText('Crédito línea 2'), '0.1000'); await user.click(screen.getByRole('button', { name: 'Confirmar asiento' }));
    expect(await screen.findByText('No hay periodo abierto.')).toBeInTheDocument(); expect(screen.getByLabelText('Débito línea 1')).toHaveValue('0.1000'); await user.click(screen.getByRole('button', { name: 'Confirmar asiento' }));
    await waitFor(() => expect(fetchMock.mock.calls.filter(([, options]) => options.method === 'POST')).toHaveLength(2)); const payloads = fetchMock.mock.calls.filter(([, options]) => options.method === 'POST').map(([, options]) => JSON.parse(String(options.body))); expect(payloads[0].operationId).toMatch(/^[\da-f-]{36}$/); expect(payloads[0].operationId).toBe(payloads[1].operationId); expect(payloads[0].lines[0].debit).toBe('0.1000'); expect(payloads[0].lines[1].credit).toBe('0.1000'); expect(screen.queryByText('Asiento confirmado por la API.')).not.toBeInTheDocument();
  });
  it('descarta una consulta tardía de la empresa anterior', async () => {
    let resolveFirst: ((value: Response) => void) | undefined;
    setupFetch((_path, options) => new Headers(options.headers).get('X-Company-Id') === 'company-1' ? new Promise(resolve => { resolveFirst = resolve; }) : json(envelope([{ id: 'b', name: 'Cuenta empresa dos', code: '200' }])));
    const { rerender } = render(<ResourcePage key="company-1" module={accountModule} {...context}/>);
    rerender(<ResourcePage key="company-2" module={accountModule} {...context} companyId="company-2"/>); expect(await screen.findByText('Cuenta empresa dos')).toBeInTheDocument(); await act(async () => { resolveFirst?.(json(envelope([{ id: 'a', name: 'Cuenta empresa uno', code: '100' }]))); }); expect(screen.queryByText('Cuenta empresa uno')).not.toBeInTheDocument();
  });
});
