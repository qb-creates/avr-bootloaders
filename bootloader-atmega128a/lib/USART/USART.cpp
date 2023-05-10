#include <USART.h>

FILE transmit;

/**
 * Enables the USART transmitter and receiver.
 * @param[in] ubrr baud rate.
 * @param[in] dataBits Number of databits.
 * @param[in] parity None, Even, or Odd.
 * @param[in] stopBits Number of stop bits.
 * @param[in] clock Clock polarity.
 * @param[in] mode Asynchronous or Synchronous mode.
 * */
void USART0::configure(int ubrr, Databits dataBits, Parity parity, StopBits stopBits, ClkPolarity clock, Mode mode)
{
    // Separates the three UCSZ bits in the databits enum. Shift the bit by the proper ammount to set it in the register.
    int ucsz02 = ((static_cast<int>(dataBits) & 0x04) == 0x04) << UCSZ02;
    int ucsz01 = ((static_cast<int>(dataBits) & 0x02) == 0x02) << UCSZ01;
    int ucsz00 = (static_cast<int>(dataBits) & 0x01) << UCSZ00;

    // Separates the two UMP bits in the Parity enum. Shift the bit by the proper ammount to set it in the register.
    int ump01 = ((static_cast<int>(parity) & 0x02) == 0x02) << UPM01;
    int ump00 = (static_cast<int>(parity) & 0x01) << UPM00;

    // Shift the bit by the proper ammount to set it in the register.
    int usbs0 = static_cast<int>(stopBits) << USBS0;
    int umsel0 = static_cast<int>(mode) << UMSEL0;
    int ucpol0 = static_cast<int>(clock) << UCPOL0;

    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR0H = (unsigned char)(ubrr >> 8);

    // Put the remaining part of the baud number here
    UBRR0L = (unsigned char)ubrr;

    // Configures the Registers
    UCSR0B = (1 << RXCIE0) | (1 << RXEN0) | (1 << TXEN0) | ucsz02;
    UCSR0C = umsel0 | ump01 | ump00 | usbs0 | ucsz01 | ucsz00 | ucpol0;
}

/**
 * Retrieve data from the UDR0 Register
 * @return Returns the data in the UDR0 register.
 * */
unsigned char USART0::receiveData(void)
{
    while (!(UCSR0A & (1 << RXC0)))
    {
    }
    return UDR0;
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * */
void USART0::transmitData(unsigned char data)
{
    // Wait until the Transmitter is ready
    while (!(UCSR0A & (1 << UDRE0)))
    {
    }

    // Sends out data
    UDR0 = data;
}

int USART0::usartPutChar(char data, FILE *stream)
{
    if (data == '\n')
        usartPutChar('\r', stream);

    // wait for UDR to be clear
    loop_until_bit_is_set(UCSR0A, UDRE0);

    // send the character
    UDR0 = data;
    return 0;
}

/**
 * Enables the USART transmitter and receiver.
 * @param[in] ubrr baud rate.
 * @param[in] dataBits Number of databits.
 * @param[in] parity None, Even, or Odd.
 * @param[in] stopBits Number of stop bits.
 * @param[in] clock Clock polarity.
 * @param[in] mode Asynchronous or Synchronous mode.
 * */
void USART1::configure(int ubrr, Databits dataBits, Parity parity, StopBits stopBits, ClkPolarity clock, Mode mode)
{
    // Separates the three UCSZ bits in the databits enum. Shift the bit by the proper ammount to set it in the register.
    int ucsz12 = ((static_cast<int>(dataBits) & 0x04) == 0x04) << UCSZ12;
    int ucsz11 = ((static_cast<int>(dataBits) & 0x02) == 0x02) << UCSZ11;
    int ucsz10 = (static_cast<int>(dataBits) & 0x01) << UCSZ10;

    // Separates the two UMP bits in the Parity enum. Shift the bit by the proper ammount to set it in the register.
    int ump11 = ((static_cast<int>(parity) & 0x02) == 0x02) << UPM11;
    int ump10 = (static_cast<int>(parity) & 0x01) << UPM10;

    // Shift the bit by the proper ammount to set it in the register.
    int usbs1 = static_cast<int>(stopBits) << USBS1;
    int umsel1 = static_cast<int>(mode) << UMSEL1;
    int ucpol1 = static_cast<int>(clock) << UCPOL1;

    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR1H = (unsigned char)(ubrr >> 8);

    // Put the remaining part of the baud number here
    UBRR1L = (unsigned char)ubrr;

    // Configures the Registers
    UCSR1B = (1 << RXCIE1) | (1 << RXEN1) | (1 << TXEN1) | ucsz12;
    UCSR1C = umsel1 | ump11 | ump10 | usbs1 | ucsz11 | ucsz10 | ucpol1;

    transmit.put = USART1::usartPutChar;
    transmit.get = NULL;
    transmit.flags = _FDEV_SETUP_WRITE;
    stdout = &transmit;
}

/**
 * Retrieve data from the UDR1 Register
 * @return Returns the data in the UDR1 register.
 * */
unsigned char USART1::receiveData(void)
{
    while (!(UCSR1A & (1 << RXC1)))
    {
    }
    return UDR1;
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * */
void USART1::transmitData(unsigned char data)
{
    // Wait until the Transmitter is ready
    while (!(UCSR1A & (1 << UDRE1)))
    {
    }

    // Sends out data
    UDR1 = data;
}

int USART1::usartPutChar(char data, FILE *stream)
{
    if (data == '\n')
    {
        usartPutChar('\r', stream);
    }

    // wait for UDR to be clear
    loop_until_bit_is_set(UCSR1A, UDRE1);

    // send the character
    UDR1 = data;
    return 0;
}