namespace GreenGainsBackend.Domain;

public enum OBISCode
{
    code_1_7_0 = 170,   // Active Power (Total)
    code_1_8_0 = 180,   // Energy Consumption (Total)
    code_2_7_0 = 270,   // Reactive Power (Total)
    code_2_8_0 = 280,   // Reactive Energy (Total)
    code_3_8_0 = 380,   // Reactive Energy (Phase 3)
    code_4_8_0 = 480,   // Energy Consumption (Reverse)
    code_16_7_0 = 1670,  // Apparent Power (Total)
    code_31_7_0 = 3170,  // Current (Phase 1)
    code_32_7_0 = 3270,  // Voltage (Phase 1)
    code_51_7_0 = 5170,  // Current (Phase 2)
    code_52_7_0 = 5270,  // Voltage (Phase 2)
    code_71_7_0 = 7170,  // Current (Phase 3)
    code_72_7_0 = 7270   // Voltage (Phase 3)
}
