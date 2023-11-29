#ifndef BUTTONUTILITY_H
#define BUTTONUTILITY_H

#include <stdbool.h>
#include <stdint.h>

bool pressAndHold(volatile uint8_t *inputPins, uint8_t pin, uint8_t holdTime);

#endif