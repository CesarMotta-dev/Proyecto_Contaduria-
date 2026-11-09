import { useEffect, useState } from "react";
import { request } from "../services/api";

export function useApi<T>(
  path: string,
  token: string,
  companyId: string,
  revision = 0,
) {
  const [state, setState] = useState<{
    data?: T;
    error?: unknown;
    loading: boolean;
  }>({ loading: true });
  useEffect(() => {
    const controller = new AbortController();
    setState({ loading: true });
    request<T>(path, token, companyId, { signal: controller.signal })
      .then((data) => {
        if (!controller.signal.aborted) setState({ data, loading: false });
      })
      .catch((error) => {
        if (!controller.signal.aborted) setState({ error, loading: false });
      });
    return () => controller.abort();
  }, [path, token, companyId, revision]);
  return state;
}
