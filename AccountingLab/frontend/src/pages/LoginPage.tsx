import { useState } from "react";
import type { FormEvent } from "react";
import { ArrowRight, BookOpen, Layers3 } from "lucide-react";
import type { Session } from "../types/api";
import { request } from "../services/api";
import { ErrorNotice } from "../components/Feedback";

export function LoginPage({
  onLogin,
}: {
  onLogin: (session: Session) => void;
}) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<unknown>();
  const [loading, setLoading] = useState(false);
  async function submit(event: FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(undefined);
    try {
      const session = await request<Session>(
        "/auth/login",
        undefined,
        undefined,
        {
          method: "POST",
          body: JSON.stringify({ email: email.trim(), password }),
        },
      );
      if (!session.accessToken || !session.companies?.length)
        throw new Error(
          "El usuario no tiene una empresa autorizada. Consulta al administrador.",
        );
      onLogin(session);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }
  return (
    <div className="login-page">
      <section className="login-intro">
        <div className="brand">
          <span className="brand-mark">
            <Layers3 size={24} />
          </span>
          <span>
            Accounting<span className="brand-light">Lab</span>
          </span>
        </div>
        <div className="login-story">
          <span className="eyebrow">Contabilidad con propósito académico</span>
          <h1>
            Entiende el negocio.
            <br />
            Diseña la solución.
          </h1>
          <p>
            Un espacio para llevar la contabilidad, hacer buenas preguntas y
            desarrollar criterio de diseño.
          </p>
          <div className="login-stats">
            <div>
              <strong>20</strong>
              <span>módulos de negocio</span>
            </div>
            <div>
              <strong>22</strong>
              <span>desafíos de diseño</span>
            </div>
          </div>
        </div>
        <p className="login-caption">
          <BookOpen size={17} />
          Laboratorio universitario · Datos de demostración ficticios
        </p>
      </section>
      <main className="login-form-area">
        <div className="login-form">
          <span className="eyebrow">Bienvenido a tu espacio de trabajo</span>
          <h2>Iniciar sesión</h2>
          <p>Usa las credenciales asignadas por tu docente o administrador.</p>
          <form onSubmit={submit}>
            <label className="field">
              <span>Correo electrónico</span>
              <input
                type="email"
                autoComplete="username"
                required
                placeholder="tu.correo@institucion.edu"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </label>
            <label className="field">
              <span>Contraseña</span>
              <input
                type="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </label>
            <ErrorNotice error={error} />
            <button className="button login-button" disabled={loading}>
              {loading ? "Validando acceso…" : "Entrar a AccountingLab"}
              <ArrowRight size={18} />
            </button>
          </form>
          <p className="small">
            Cada sesión opera dentro de una empresa autorizada. Las funciones
            académicas pendientes se identifican en la aplicación.
          </p>
        </div>
      </main>
    </div>
  );
}
