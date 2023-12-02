#ifndef BOOTLOADUTILITY_H
#define BOOTLOADUTILITY_H

#include <avr/wdt.h>
#include "USART.h"

const uint8_t uploadCompleteByte;
const uint8_t uploadCompleteCode;
const uint8_t uploadeFailedCode;
uint8_t *bootloaderStatusAddress;

void startBootloadProcess(void);
void startBootloadIndicator(void);
void writeProgramDataToFlash(uint8_t *buf);

#endif