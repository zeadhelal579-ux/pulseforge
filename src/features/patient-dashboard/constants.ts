/** Matches the AD8232/ESP32 capture rate referenced across the architecture docs. */
export const ECG_SAMPLE_RATE_HZ = 250;

/** Rolling window shown on the live strip. */
export const ECG_WINDOW_SECONDS = 4;
export const ECG_WINDOW_SIZE = ECG_SAMPLE_RATE_HZ * ECG_WINDOW_SECONDS;

/** Static calibration readout under the canvas — standard clinical values,
 *  not computed from anything (confirm these match actual device firmware
 *  config with the hardware/backend team before treating them as ground truth). */
export const ECG_CALIBRATION = {
  paperSpeedMmPerSec: 25,
  gainMmPerMv: 10,
  sampleRateHz: ECG_SAMPLE_RATE_HZ,
};

/** Grid minor-square size in canvas pixels — a *visual* 1:5 minor:major ratio
 *  matching real ECG paper proportions, not a physical millimeter measurement. */
export const ECG_GRID_MINOR_PX = 12;
export const ECG_GRID_MAJOR_EVERY = 5;
