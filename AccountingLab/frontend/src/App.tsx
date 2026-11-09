import { useState } from "react";
import { NavLink, Route, Routes } from "react-router-dom";
import {
  Bell,
  BookOpen,
  Building2,
  ChevronRight,
  CircleDollarSign,
  ClipboardCheck,
  FileChartColumn,
  FileOutput,
  FileText,
  GraduationCap,
  Landmark,
  Layers3,
  LayoutDashboard,
  ListTree,
  LogOut,
  Menu,
  PanelLeftClose,
  RefreshCw,
  Settings,
  Users,
  Wallet,
} from "lucide-react";
import type { Company, Page, Session } from "./types/api";
import { modules } from "./services/modules";
import { request } from "./services/api";
import { ErrorNotice } from "./components/Feedback";
import { Dashboard } from "./pages/Dashboard";
import { ResourcePage } from "./pages/ResourcePage";
import { JournalPage } from "./pages/JournalPage";
import { LedgerPage } from "./pages/LedgerPage";
import { ChallengesPage } from "./pages/ChallengesPage";
import { ExportsPage, ReportsPage } from "./pages/ReportsPage";
import { LoginPage } from "./pages/LoginPage";

const navigation = [
  {
    label: "Espacio de trabajo",
    items: [
      { path: "/", label: "Dashboard", icon: LayoutDashboard },
      { path: "/journal-entries", label: "Asientos contables", icon: BookOpen },
      { path: "/journal", label: "Libro diario", icon: FileText },
      { path: "/ledger", label: "Libro mayor", icon: Landmark },
      { path: "/accounts", label: "Cuentas contables", icon: ListTree },
      { path: "/periods", label: "Periodos y cierre", icon: ClipboardCheck },
    ],
  },
  {
    label: "Gestión financiera",
    items: [
      { path: "/invoices", label: "Facturación", icon: FileText },
      { path: "/payments", label: "Pagos y cobros", icon: Wallet },
      { path: "/receivables", label: "Por cobrar", icon: CircleDollarSign },
      { path: "/payables", label: "Por pagar", icon: CircleDollarSign },
      {
        path: "/bank-statements",
        label: "Extractos bancarios",
        icon: Landmark,
      },
      {
        path: "/bank-reconciliation",
        label: "Conciliación",
        icon: ClipboardCheck,
      },
      { path: "/budgets", label: "Presupuestos", icon: FileChartColumn },
      { path: "/reports", label: "Reportes", icon: FileChartColumn },
      { path: "/exports", label: "Exportaciones", icon: FileOutput },
    ],
  },
  {
    label: "Administración",
    items: [
      { path: "/approvals", label: "Aprobaciones", icon: ClipboardCheck },
      { path: "/audit", label: "Auditoría", icon: FileText },
      { path: "/notifications", label: "Notificaciones", icon: Bell },
      { path: "/companies", label: "Empresas", icon: Building2 },
      { path: "/users", label: "Usuarios", icon: Users },
      { path: "/third-parties", label: "Terceros", icon: Users },
      { path: "/cost-centers", label: "Centros de costos", icon: ListTree },
      { path: "/tax-rules", label: "Impuestos", icon: Settings },
    ],
  },
];
function storedSession(): Session | null {
  try {
    const data = JSON.parse(
      sessionStorage.getItem("accountinglab-session") || "null",
    ) as Session | null;
    return data?.accessToken && data.user?.id && data.companies?.length
      ? data
      : null;
  } catch {
    return null;
  }
}
export function App() {
  const [session, setSession] = useState<Session | null>(storedSession);
  const [activeId, setActiveId] = useState(
    () => sessionStorage.getItem("accountinglab-company") || "",
  );
  const [mobileOpen, setMobileOpen] = useState(false);
  const [refreshingCompanies, setRefreshingCompanies] = useState(false);
  const [companyError, setCompanyError] = useState<unknown>();
  function login(data: Session) {
    sessionStorage.setItem("accountinglab-session", JSON.stringify(data));
    sessionStorage.setItem("accountinglab-company", data.companies[0].id);
    setActiveId(data.companies[0].id);
    setSession(data);
  }
  function logout() {
    sessionStorage.removeItem("accountinglab-session");
    sessionStorage.removeItem("accountinglab-company");
    setSession(null);
  }
  async function refreshCompanies() {
    if (!session) return;
    setRefreshingCompanies(true);
    setCompanyError(undefined);
    try {
      const result = await request<Page<Company>>(
        "/companies?page=1&pageSize=100",
        session.accessToken,
        activeId || session.companies[0].id,
      );
      if (!result.items.length)
        throw new Error(
          "La API no devolvió empresas autorizadas. Revisa tu acceso con el administrador.",
        );
      const next = { ...session, companies: result.items };
      setSession(next);
      sessionStorage.setItem("accountinglab-session", JSON.stringify(next));
    } catch (error) {
      setCompanyError(error);
    } finally {
      setRefreshingCompanies(false);
    }
  }
  if (!session) return <LoginPage onLogin={login} />;
  const company =
    session.companies.find((c) => c.id === activeId) || session.companies[0];
  const context = {
    token: session.accessToken,
    companyId: company.id,
    currency: company.currency,
    role: company.role,
  };
  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">
        Saltar al contenido
      </a>
      {mobileOpen && (
        <button
          className="sidebar-overlay"
          onClick={() => setMobileOpen(false)}
          aria-label="Cerrar menú"
        />
      )}
      <aside className={`sidebar ${mobileOpen ? "is-open" : ""}`}>
        <div className="sidebar-brand">
          <div className="brand">
            <span className="brand-mark">
              <Layers3 size={22} />
            </span>
            <span>
              Accounting<span className="brand-light">Lab</span>
            </span>
          </div>
          <button
            className="mobile-only icon-button"
            onClick={() => setMobileOpen(false)}
            aria-label="Ocultar menú"
          >
            <PanelLeftClose size={20} />
          </button>
        </div>
        <div className="workspace-label">
          <span className="workspace-dot" />
          Entorno académico
        </div>
        <nav aria-label="Navegación principal" className="sidebar-nav">
          {navigation.map((group) => (
            <div className="nav-group" key={group.label}>
              <p>{group.label}</p>
              {group.items.map((item) => (
                <NavLink
                  key={item.path}
                  to={item.path}
                  end={item.path === "/"}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) =>
                    isActive ? "nav-link active" : "nav-link"
                  }
                >
                  <item.icon size={17} />
                  <span>{item.label}</span>
                </NavLink>
              ))}
            </div>
          ))}
          <div className="nav-group learning-nav">
            <NavLink
              to="/challenges"
              onClick={() => setMobileOpen(false)}
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              <GraduationCap size={20} />
              <span>Desafíos de diseño</span>
              <span className="nav-number">22</span>
            </NavLink>
          </div>
        </nav>
        <div className="sidebar-footer">
          <div>
            <strong>{session.user.name}</strong>
            <span>{company.role}</span>
          </div>
          <button
            className="icon-button"
            onClick={logout}
            aria-label="Cerrar sesión"
          >
            <LogOut size={18} />
          </button>
        </div>
      </aside>
      <div className="workspace">
        <header className="topbar">
          <div className="topbar-context">
            <button
              className="mobile-only icon-button"
              onClick={() => setMobileOpen(true)}
              aria-label="Abrir menú"
            >
              <Menu size={20} />
            </button>
            <span className="breadcrumb">
              AccountingLab <ChevronRight size={14} /> Gestión empresarial
            </span>
          </div>
          <div className="company-control">
            <Building2 size={17} />
            <label>
              <span className="sr-only">Empresa activa</span>
              <select
                aria-label="Empresa activa"
                value={company.id}
                onChange={(e) => {
                  setActiveId(e.target.value);
                  sessionStorage.setItem(
                    "accountinglab-company",
                    e.target.value,
                  );
                }}
              >
                {session.companies.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name} · {item.currency}
                  </option>
                ))}
              </select>
            </label>
            <button
              className="icon-button"
              title="Actualiza la lista después de crear una empresa"
              aria-label="Actualizar empresas"
              onClick={refreshCompanies}
              disabled={refreshingCompanies}
            >
              <RefreshCw
                size={16}
                className={refreshingCompanies ? "spin" : ""}
              />
            </button>
          </div>
        </header>
        <main id="main-content" className="main-content" key={company.id}>
          <ErrorNotice error={companyError} />
          <Routes>
            <Route path="/" element={<Dashboard {...context} />} />
            {modules.map((module) => (
              <Route
                key={module.path}
                path={`/${module.path}`}
                element={
                  <ResourcePage
                    key={`${company.id}-${module.path}`}
                    {...context}
                    module={module}
                  />
                }
              />
            ))}
            <Route
              path="/journal-entries"
              element={<JournalPage {...context} />}
            />
            <Route
              path="/journal"
              element={<JournalPage {...context} readOnly />}
            />
            <Route path="/ledger" element={<LedgerPage {...context} />} />
            <Route path="/reports" element={<ReportsPage {...context} />} />
            <Route path="/exports" element={<ExportsPage {...context} />} />
            <Route
              path="/challenges"
              element={<ChallengesPage {...context} />}
            />
            <Route
              path="*"
              element={
                <div className="empty">
                  <h1>Página no encontrada</h1>
                  <p>Elige un módulo del menú para continuar.</p>
                  <NavLink to="/" className="button">
                    Volver al dashboard
                  </NavLink>
                </div>
              }
            />
          </Routes>
          <footer className="page-footer">
            <span>AccountingLab</span>
            <span>Laboratorio de contabilidad y diseño de software</span>
            <a
              href="https://refactoring.guru/es/design-patterns/catalog"
              target="_blank"
              rel="noreferrer"
            >
              Catálogo de patrones ↗
            </a>
          </footer>
        </main>
      </div>
    </div>
  );
}
