#ifndef BOOTLOADUTILITY_H
#define BOOTLOADUTILITY_H

#include <stdint.h>
#include <USART.h>
#include <avr/wdt.h>

const uint8_t lastPageIndicator;
const uint8_t uploadCompleteCode;
const uint8_t uploadeFailedCode;
uint8_t *bootloaderStatusAddress;

void startBootloadProcess(void);
void startBootloadIndicator(void);
void writePageDataToFlash(uint8_t *buf);

#endif