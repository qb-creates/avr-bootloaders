#ifndef BOOTLOADER_H
#define BOOTLOADER_H

int pressAndHold(int holdTime);
void startBootloaderIndicator();
void stopBootloaderIndicator();
char writeProgramDataToFlash(volatile unsigned char *buf);

#endif