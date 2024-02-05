# ATmega1284 Bootloader
This repository contains a custom bootloader designed for ATmega1284 microcontrollers, enabling easy program uploads using USART communication.

## 1. Development Environment<a name="software"></a>
- Developed using VS Code with the PlatformIO extension: https://docs.platformio.org/en/latest/what-is-platformio.html
- AVRDUDE (Flash Uploader): https://github.com/avrdudes/avrdude

## Configuration Tips

1. **Fuse Bits Configuration:**
   - Configure fuse bits to enable the boot reset vector.
   - Choose the desired boot flash section size. The bootloader can run on the 512-word boot flash section.
  
## Linker Options and Boot Flash Section Configuration
Make sure that the start address for the hex file is set to the address associated with the microcontrollers configured boot flash section.
To properly configure the linker options and other PlatformIO settings, follow these steps:

1. Open the `platformio.ini` file in the root directory of your PlatformIO project.
2. Add a `[env]` section or modify the existing `[env]` section for ATmega1284 (e.g., `[env:atmega1284]`).
3. Use the `build_flags` option to specify the linker options. For setting the start address of the hex file, use the `-Wl,-Ttext=address` option.

   ```ini
   [env:atmega1284]
   platform = atmelavr
   board = ATmega1284
   board_build.f_cpu = ...
   build_flags =
     -Wl,-section-start=.text=<Address of the boot flash section in bytes not words>

## Communication

- USART communication is used for bootloader operation.
- USART0 is utilized for ATmega1284.

## Size and SRAM Usage

- The bootloader is less than 1024 bytes in size.
- Utilizes only 10 bytes of SRAM.

## Getting Started

1. Clone this repository:
   ```bash
   git clone https://github.com/your-username/atmega1284-bootloader.git
