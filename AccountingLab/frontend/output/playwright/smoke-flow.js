async (page) => {
  const errors = [];
  const responses = [];
  const onPageError = (error) => errors.push(error.message);
  const onResponse = (response) => {
    if (response.url().includes('/api/')) {
      responses.push({ path: new URL(response.url()).pathname, status: response.status() });
    }
  };
  page.on('pageerror', onPageError);
  page.on('response', onResponse);
  await page.screenshot({ path: 'output/playwright/dashboard-real.png', timeout: 30000 });
  await page.getByRole('link', { name: 'Cuentas contables', exact: true }).click();
  await page.getByRole('heading', { name: 'Cuentas contables', exact: true }).waitFor();
  await page.getByRole('button', { name: 'Nuevo registro', exact: true }).click();
  await page.getByLabel('Código *', { exact: true }).waitFor();
  const code = '999' + Date.now().toString().slice(-7);
  const name = 'SMOKE navegador ' + code;
  await page.getByLabel('Código *', { exact: true }).fill(code);
  await page.getByLabel('Nombre *', { exact: true }).fill(name);
  await page.getByRole('button', { name: 'Guardar registro', exact: true }).click();
  await page.getByRole('status').filter({ hasText: 'Registro guardado' }).waitFor({ timeout: 20000 });
  await page.getByText(name, { exact: true }).waitFor();
  await page.reload();
  await page.getByText(name, { exact: true }).waitFor({ timeout: 20000 });
  await page.getByLabel('Empresa activa').selectOption({ label: 'Empresa Ficticia Boreal · COP' });
  await page.getByRole('status').waitFor({ state: 'hidden' });
  if (await page.getByText(name, { exact: true }).count()) {
    throw new Error('Datos de la primera empresa visibles en la segunda.');
  }
  await page.getByLabel('Empresa activa').selectOption({ label: 'Empresa Ficticia Aurora · COP' });
  await page.getByText(name, { exact: true }).waitFor({ timeout: 20000 });
  await page.getByRole('link', { name: 'Desafíos de diseño 22', exact: true }).click();
  await page.getByText('22 desafíos', { exact: true }).waitFor();
  const challengeCount = await page.getByRole('article').count();
  if (challengeCount !== 22) throw new Error('Catálogo incompleto: ' + challengeCount);
  await page.getByRole('button', { name: 'DP-08', exact: true }).click();
  await page.getByRole('alert').filter({ hasText: 'DP-08' }).waitFor({ timeout: 20000 });
  const pendingText = await page.getByRole('alert').textContent();
  if (!pendingText.includes('Composite') || !pendingText.includes('no se ha confirmado')) {
    throw new Error('Estado pendiente incoherente.');
  }
  await page.screenshot({ path: 'output/playwright/challenge-501-real.png', timeout: 30000 });
  page.off('pageerror', onPageError);
  page.off('response', onResponse);
  return {
    authenticated: true,
    createdAccount: { code, name },
    persistedAfterReload: true,
    companyIsolation: true,
    challengeCount,
    pendingChallenge: 'DP-08 Composite',
    expected501: responses.filter((r) => r.status === 501).length,
    unexpectedHttp: responses.filter((r) => r.status >= 400 && r.status !== 501),
    pageErrors: errors,
    responses,
  };
};
