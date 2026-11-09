import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { X } from "lucide-react";
import type { Field } from "../services/modules";
import type { Entity, Page } from "../types/api";
import { isDecimal, request } from "../services/api";
import { ErrorNotice } from "./Feedback";

export function EntryForm({
  fields,
  token,
  companyId,
  onSave,
  onCancel,
  title,
}: {
  fields: Field[];
  token: string;
  companyId: string;
  onSave: (payload: Record<string, unknown>) => Promise<void>;
  onCancel: () => void;
  title: string;
}) {
  const [values, setValues] = useState<Record<string, string | boolean>>(() =>
    Object.fromEntries(
      fields.map((f) => [
        f.key,
        f.kind === "checkbox" ? f.key === "isActive" : "",
      ]),
    ),
  );
  const [lookups, setLookups] = useState<Record<string, Entity[]>>({});
  const [error, setError] = useState<unknown>();
  const [validation, setValidation] = useState("");
  const [saving, setSaving] = useState(false);
  useEffect(() => {
    const controller = new AbortController();
    const resources = [
      ...new Set(fields.map((f) => f.resource).filter((x): x is string => !!x)),
    ];
    Promise.all(
      resources.map(async (resource) => {
        const result = await request<Page<Entity>>(
          `/${resource}?page=1&pageSize=100`,
          token,
          companyId,
          { signal: controller.signal },
        );
        return [resource, result.items] as const;
      }),
    )
      .then((results) => {
        if (!controller.signal.aborted) setLookups(Object.fromEntries(results));
      })
      .catch((err) => {
        if (!controller.signal.aborted) setError(err);
      });
    return () => controller.abort();
  }, [fields, token, companyId]);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setValidation("");
    setError(undefined);
    const payload: Record<string, unknown> = {};
    for (const field of fields) {
      const raw = values[field.key];
      const value = typeof raw === "string" ? raw.trim() : raw;
      if (field.required && !value) {
        setValidation(`Completa ${field.label.toLowerCase()}.`);
        return;
      }
      if (
        field.kind === "decimal" &&
        value &&
        !isDecimal(String(value), field.key === "rate" ? 6 : 4)
      ) {
        setValidation(
          `${field.label}: usa punto decimal y máximo ${field.key === "rate" ? 6 : 4} decimales.`,
        );
        return;
      }
      if (field.kind === "integer" && !/^\d{4}$/.test(String(value))) {
        setValidation("Escribe un año de cuatro dígitos.");
        return;
      }
      payload[field.key] =
        field.kind === "integer"
          ? parseInt(String(value), 10)
          : value === ""
            ? null
            : value;
    }
    if (
      payload.startDate &&
      payload.endDate &&
      String(payload.endDate) < String(payload.startDate)
    ) {
      setValidation("La fecha final debe ser igual o posterior a la inicial.");
      return;
    }
    if (
      payload.issueDate &&
      payload.dueDate &&
      String(payload.dueDate) < String(payload.issueDate)
    ) {
      setValidation("El vencimiento debe ser igual o posterior a la emisión.");
      return;
    }
    if (
      payload.validFrom &&
      payload.validTo &&
      String(payload.validTo) < String(payload.validFrom)
    ) {
      setValidation(
        "La vigencia final debe ser igual o posterior a la inicial.",
      );
      return;
    }
    setSaving(true);
    try {
      await onSave(payload);
    } catch (err) {
      setError(err);
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="form-panel" aria-label={title}>
      <div className="panel-title">
        <h2>{title}</h2>
        <button
          className="icon-button"
          type="button"
          onClick={onCancel}
          aria-label="Cerrar formulario"
          disabled={saving}
        >
          <X size={20} />
        </button>
      </div>
      <form onSubmit={submit} className="form-grid">
        {fields.map((field) => (
          <label
            className={field.kind === "checkbox" ? "check-field" : "field"}
            key={field.key}
          >
            {field.kind !== "checkbox" && (
              <span>
                {field.label}
                {field.required && <span aria-hidden="true"> *</span>}
              </span>
            )}
            {field.kind === "checkbox" ? (
              <>
                <input
                  type="checkbox"
                  checked={Boolean(values[field.key])}
                  onChange={(e) =>
                    setValues({ ...values, [field.key]: e.target.checked })
                  }
                />
                {field.label}
              </>
            ) : field.kind === "select" ? (
              <select
                required={field.required}
                value={String(values[field.key] || "")}
                onChange={(e) =>
                  setValues({ ...values, [field.key]: e.target.value })
                }
              >
                <option value="">Seleccionar…</option>
                {field.options?.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
                {field.resource &&
                  lookups[field.resource]?.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.code ? `${item.code} · ` : ""}
                      {String(item.name || item.reference || item.id)}
                    </option>
                  ))}
              </select>
            ) : (
              <input
                required={field.required}
                type={
                  field.kind === "date"
                    ? "date"
                    : field.kind === "password"
                      ? "password"
                      : field.kind === "email"
                        ? "email"
                        : "text"
                }
                autoComplete={
                  field.kind === "password" ? "new-password" : "off"
                }
                inputMode={
                  field.kind === "decimal"
                    ? "decimal"
                    : field.kind === "integer"
                      ? "numeric"
                      : undefined
                }
                value={String(values[field.key] || "")}
                onChange={(e) =>
                  setValues({ ...values, [field.key]: e.target.value })
                }
                maxLength={field.kind === "password" ? 128 : 250}
              />
            )}
            {field.hint && <small>{field.hint}</small>}
          </label>
        ))}
        <div className="form-footer">
          <p className="small">
            * Campos obligatorios. La API valida las reglas de negocio.
          </p>
          {validation && (
            <p className="validation" role="alert">
              {validation}
            </p>
          )}
          <ErrorNotice error={error} />
          <div className="button-row">
            <button
              type="button"
              className="button secondary"
              onClick={onCancel}
              disabled={saving}
            >
              Cancelar
            </button>
            <button className="button" disabled={saving}>
              {saving ? "Guardando…" : "Guardar registro"}
            </button>
          </div>
        </div>
      </form>
    </section>
  );
}
