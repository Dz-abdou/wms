import { afterEach, describe, expect, it, vi } from "vitest";
import {
  clearAccessToken,
  getAccessToken,
  refreshAccessToken,
} from "./authApi";

describe("refreshAccessToken", () => {
  afterEach(() => {
    clearAccessToken();
    vi.unstubAllGlobals();
  });

  it("shares one request across concurrent refresh attempts", async () => {
    let resolveResponse: (response: Response) => void = () => {};
    const response = new Promise<Response>((resolve) => {
      resolveResponse = resolve;
    });
    const fetchMock = vi.fn(() => response);
    vi.stubGlobal("fetch", fetchMock);

    const first = refreshAccessToken();
    const second = refreshAccessToken();

    expect(fetchMock).toHaveBeenCalledTimes(1);

    resolveResponse(
      new Response(JSON.stringify({ accessToken: "restored-access-token" }), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );

    await Promise.all([first, second]);

    expect(getAccessToken()).toBe("restored-access-token");
  });
});
