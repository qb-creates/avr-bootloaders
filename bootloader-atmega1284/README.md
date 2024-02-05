# ATmega1284 Bootloader
This repository contains a custom bootloader designed for ATmega1284 microcontrollers, enabling easy program uploads using USART communication.

## 1. Development Environment<a name="software"></a>
- Developed using VS Code with the PlatformIO extension: https://docs.platformio.org/en/latest/what-is-platformio.html
- AVRDUDE (Flash Uploader): https://github.com/avrdudes/avrdude

## Getting Started
1. Clone this repository:
   ```bash
   git clone https://github.com/your-username/atmega1284-bootloader.git
   
## Configuration Tips

1. **Fuse Bits Configuration:**
   - Configure fuse bits to enable the boot reset vector.
   - Choose the desired boot flash section size. The bootloader can run on the 512-word boot flash section.
  
## PlatformIO Environment Settings

The [env] section of a platformio.ini file is used to define environments or build configurations for your project. In this section, you can specify the CPU frequency and configure linker options to set the starting address for the code to the address associated with your microcontroller's BOOTSZ fuse bits configuration.

<br>

To properly configure the environment settings and linker options, follow these steps:

1. Open the `platformio.ini` file in the root directory of the PlatformIO project.
2. Add a `[env]` section or modify the existing `[env]` section for ATmega1284 (e.g., `[env:atmega1284]`).
3. Use the `build_flags` option to specify the linker options. For setting the start address of the hex file, use the `-Wl,-section-start=.text=<starting_address>` option. 
4. Use the `board_build.f_cpu` option to specify the cpu frequency. 

   ```ini
   [env:atmega1284]
   platform = atmelavr
   board = ATmega1284
   board_build.f_cpu = <cpu_frequency>
   build_flags =
     -Wl,-section-start=.text=<starting_address>

## Communication

- USART communication is used for bootloader operation.
- USART0 is utilized for ATmega1284.

## Size and SRAM Usage

- The bootloader is less than 1024 bytes in size.
- Utilizes only 10 bytes of SRAM.
   
## Bootloader Mode
When there isn't any program data in the program section of the flash, the microcontroller will automatically enter bootloader mode. A LED connected to pin D6 will flash to indicate that that the microcontroller is in bootloader mode. At this point, you can use the <a href="https://github.com/qb-creates/qbdude">QB.DUDE Utility</a> to upload program data to the microcontroller.  The indicator led will flash slower as data is being sent to the microcontroller. After receiving all of the data the microcontroller will reset and start the program sectoin. To re-enter bootloader mode after a program has been uploaded, follow these steps:

1. Reset the microcontroller by pulling the reset pin to ground.
2. Pull PD2 low.
3. Set the reset pin back to high while PD2 is still low.
4. After 4 seconds, the microcontroller will enter bootloader mode. At this point PD2 can be set high again

This process can be easily achieved by adding push buttons to the reset pin (pulls it low when pressed) and pin D2 (pulls it low when pressed). To exit bootloader mode after a successful program upload, simply reset the microcontroller.

If data upload fails due to power loss or communication issues, the microcontroller will timeout and stay in bootloader mode until a new program is successfully uploaded.

