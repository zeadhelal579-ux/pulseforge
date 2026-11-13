import { apiClient } from '@/shared/lib/apiClient';
import type { DeviceStatusResponse, PairDeviceRequest } from '@/shared/types/api';

/** POST /api/devices/pair — backend-database-architecture.md §5. */
export async function pairDevice(payload: PairDeviceRequest): Promise<DeviceStatusResponse> {
  const { data } = await apiClient.post<DeviceStatusResponse>('/devices/pair', payload);
  return data;
}

/** GET /api/devices/{serial}/status */
export async function getDeviceStatus(serialNumber: string): Promise<DeviceStatusResponse> {
  const { data } = await apiClient.get<DeviceStatusResponse>(`/devices/${serialNumber}/status`);
  return data;
}
