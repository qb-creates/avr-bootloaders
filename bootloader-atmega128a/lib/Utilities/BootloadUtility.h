#ifndef BOOTLOADUTILITY_H
#define BOOTLOADUTILITY_H

#include <stdint.h>
#include <stdbool.h>

void startBootloaderIndicator(uint8_t frequency);
void stopBootloaderIndicator(void);
void setBootloaderIndicatorFrequency(uint8_t frequency);
uint8_t writeProgramDataToFlash(uint8_t *buf);

#endif