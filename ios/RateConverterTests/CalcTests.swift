import XCTest
@testable import RateConverter

final class CalcTests: XCTestCase {
    func testParsesNumbersAndExpressions() {
        XCTAssertEqual(Calc.eval("100"), 100)
        XCTAssertEqual(Calc.eval("1,234.5"), 1234.5)
        XCTAssertEqual(Calc.eval("１００＋５０"), 150)
        XCTAssertEqual(Calc.eval("100*3+50"), 350)
        XCTAssertEqual(Calc.eval("100×3−50÷2"), 275)
        XCTAssertEqual(Calc.eval("(1+2)*3"), 9)
        XCTAssertEqual(Calc.eval("-5"), -5)
        XCTAssertNil(Calc.eval("12+"))
        XCTAssertNil(Calc.eval("abc"))
        XCTAssertNil(Calc.eval("1/0"))
        XCTAssertNil(Calc.eval(""))
    }

    func testIsExpression() {
        XCTAssertTrue(Calc.isExpression("100*3"))
        XCTAssertFalse(Calc.isExpression("-100"))
        XCTAssertFalse(Calc.isExpression("1,000"))
    }

    func testFormatting() {
        XCTAssertEqual(Calc.format(1234.567, code: "CNY"), "1,234.57")
        XCTAssertEqual(Calc.format(1234.5, code: "JPY"), "1,235")
        XCTAssertEqual(Calc.format(0.00123456, code: "USD"), "0.001235")
        XCTAssertEqual(Calc.format(0.5, code: "USD"), "0.50")
        XCTAssertEqual(Calc.formatExact(350), "350")
        XCTAssertEqual(Calc.formatRate(0.9134), "0.9134")
        XCTAssertEqual(Calc.formatRate(7.12346), "7.1235")
        XCTAssertEqual(Calc.formatRate(149.567), "149.57")
    }

    func testCurrencies() {
        XCTAssertEqual(Currencies.defaults, ["CNY", "HKD", "JPY", "USD"])
        XCTAssertEqual(Currencies.name("HKD"), "港币")
        XCTAssertEqual(Currencies.symbol("AED"), "AE")
        XCTAssertEqual(Currencies.decimals("JPY"), 0)
        XCTAssertTrue(Currencies.isKnown("EUR"))
    }
}
