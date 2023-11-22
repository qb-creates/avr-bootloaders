#ifndef BOOTLOADER_H
#define BOOTLOADER_H
#include <stdint.h>

uint8_t pressAndHold(uint8_t holdTime);
void startBootloaderIndicator();
void stopBootloaderIndicator();
uint8_t writeProgramDataToFlash(uint8_t *buf);

#endif