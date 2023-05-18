#ifndef USART_H
#define USART_H
#include <stdio.h>
#include <avr/io.h>

// Represents our three UCSZ bits in the USCRnx Registers
enum Databits
{
  /**
   * UCSZn0: Bit 1 of the Databits enum.
   * UCSZn1: Bit 2 of the Databits enum.
   * UCSZn2: Bit 3 of the Databits enum.
   * */
  Five = 0,
  Six = 0x01,
  Seven = 0x02,
  Eight = 0x03,
  Nine = 0x07
};

// Represents our two UPM bits in the USCRnC Register
enum Parity
{
  /**
   * UPMn0: Bit 1 of the Parity enum.
   * UPMn1: Bit 2 of the Parity enum.
   * */
  None = 0x00,
  Even = 0x02,
  Odd = 0x03
};

// Represents our USPBS bit in the USCRnC Register
enum StopBits
{
  One = 0,
  Two = 1,
};

// Represents our UMSEL bit in the USCRnC Register
enum Mode
{
  Async = 0,
  Sync = 1
};

// Represents our UCPOL bit in the USCRnC Register
enum ClkPolarity
{
  RisingFalling = 0,
  FallingRising = 1
};

/**
 * @brief Static USART0 class. Can be used for serial data transmission.
 * 
 */
class USART0
{
public:
  static void configure(int ubrr, Databits dataBits, Parity parity, StopBits stopBits, ClkPolarity clock, Mode mode);
  static unsigned char receiveData(void);
  static void transmitData(unsigned char data);
  static int usartPutChar(char c, FILE *stream);
};

/**
 * @brief Static USART1 class. Can be used for serial data transmission.
 * 
 */
class USART1
{
public:
  static void configure(int ubrr, Databits dataBits, Parity parity, StopBits stopBits, ClkPolarity clock, Mode mode);
  static unsigned char receiveData(void);
  static void transmitData(unsigned char data);
  static int usartPutChar(char c, FILE *stream);
};
#endif