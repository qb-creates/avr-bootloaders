#ifndef BUTTONPRESSUTILITY_H
#define BUTTONPRESSUTILITY_H

#include <stdbool.h>
#include <stdint.h>

bool pressAndHold(volatile uint8_t *ddr, uint8_t pin, uint8_t holdTime);

#endif