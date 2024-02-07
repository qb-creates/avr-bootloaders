# ATmega1284 Bootloader
This repository contains a custom bootloader designed for the ATmega1284 microcontroller, enabling easy program uploads using USART communication. The design of this bootloader was intended for compatibility with the <a href="https://github.com/qb-creates/qbdude">QB.DUDE Utility</a>.
      
### Development Environment<a name="development"></a>
- Developed using VS Code with the PlatformIO extension: https://docs.platformio.org/en/latest/what-is-platformio.html
- AVRDUDE (Flash Uploader): https://github.com/avrdudes/avrdude

### Communication Protocol<a name="communication"></a>

- USART communication is used for bootloader operation.
- USART0 is utilized for ATmega1284.

### Flash Size and SRAM Usage<a name="size"></a>

- The bootloader size is 976 bytes.
- Uses only 10 bytes of SRAM.

<h2> Table of Contents</h2>

1. [Enable BOOTRST Vector](#enableboot)
    - [Fuse Bit Configuration](#fusebits)
2. [Installing VS Code and PlatformIO](#installing)
2. [PlatformIO Environment Settings](#environment)
3. [Bootloader Mode](#bootmode)

## 1. Enable BOOTRST Vector<a name="enableboot"></a>

The BOOTRST vector allows the microcontroller to start execution from the bootloader section upon a reset. To utilize the BOOTRST vector on your ATmega1284 microcontroller, ensure that the relevant fuse bits are set correctly. This is neccessary for the bootloader to work properly.

### Fuse Bit Configuration<a name="fusebits"></a>

- **BOOTSZ**: Depending on the bootloader size requirement, set the `BOOTSZ` fuse bits accordingly. This bootloader is less than 1024 Bytes in size so any combination of BOOTSZ will work.
- **BOOTRST**: Enable the `BOOTRST` fuse bit to activate the BOOTRST vector. This ensures that the microcontroller begins execution from the bootloader section upon a reset.

You can use tools like <a href="https://github.com/avrdudes/avrdude">avrdude</a> to set the fuse bits. Refer to the ATmega1284 datasheet for the specific fuse bit values. 
<br>

Example command:

```bash
avrdude -c <programmer> -p <part_number> -U lfuse:w:<low_bits>:m -U hfuse:w:<high_bits>:m -U efuse:w:<extended_bits>:m
```

<br>

## 2. Installing VS Code and PlatformIO<a name="installing"></a>
1. Install <a href="https://code.visualstudio.com/download">VS Code</a>.
2. Open VS Code
3. Go to the Extensions view by clicking on the square icon on the sidebar or by pressing `ctrl + shift + x`.
4. Install the PlatformIO and C/C++ extensions
5. Clone this repository:
   ```bash
   git clone https://github.com/qb-creates/avr-bootloaders
6. Navigate to the bootloader-atmega1284 directory.
7. Right click anywhere in the directory and select open with VS Code.

### Additional Notes:
- After installing the PlatformIO extension, you'll need to follow its setup wizard to configure your environment and install necessary dependencies.
- Make sure you have a working C/C++ compiler installed on your system for the C/C++ extension to work properly. Depending on your platform, you may need to install GCC, Clang, or another compiler toolchain.
  
<br>

## 3. PlatformIO Environment Settings<a name="environment"></a>

The [env] section of a platformio.ini file is used to define environments or build configurations for your project. It can be found in the root directory of the project. In this section, you can specify the CPU frequency and configure linker options to set the starting address for the code to the address associated with your microcontroller's BOOTSZ fuse bits configuration. These options need to be set before we build the project

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
<br>

After setting the options, use `ctrl + shift + b` to build the solution and generate the hex file. You can use tools like <a href="https://github.com/avrdudes/avrdude">avrdude</a> to upload the bootloader to the microcontroller. The hex file will be generated in the `.pio/biuld/ATmega1284 directory`. It will be called `firmware.hex`
<br>

Example command:

```bash
avrdude -c <programmer> -p <part_number> -P PORT -b BAUD_RATE -U flash:w:<path_to_hex_file>
```

<br>

## 4. Bootloader Mode<a name="bootmode"></a>
When there isn't any program data in the program section of the flash, the microcontroller will automatically enter bootloader mode. A LED connected to pin D6 will flash to indicate that that the microcontroller is in bootloader mode. At this point, you can use the <a href="https://github.com/qb-creates/qbdude">QB.DUDE Utility</a> to upload program data to the microcontroller.  The indicator led will flash slower as data is being sent to the microcontroller. After receiving all of the data the microcontroller will reset and start the program sectoin. To re-enter bootloader mode after a program has been uploaded, follow these steps:

1. Reset the microcontroller by pulling the reset pin to ground.
2. Pull PD2 low.
3. Set the reset pin back to high while PD2 is still low.
4. After 4 seconds, the microcontroller will enter bootloader mode. At this point PD2 can be set high again

This process can be easily achieved by adding push buttons to the reset pin (pulls it low when pressed) and pin D2 (pulls it low when pressed). To exit bootloader simply reset the microcontroller. The microcontroller will then run the program that is currently loaded in it's program flash section. If no program is loaded, the microcontroller will stay in bootloader mode.

If the data upload fails due to power loss or communication issues, the microcontroller will timeout and stay in bootloader mode until a new program is successfully uploaded.

