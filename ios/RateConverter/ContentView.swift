import SwiftUI
import UIKit

struct ContentView: View {
    @EnvironmentObject var model: AppModel
    @Environment(\.scenePhase) private var scenePhase
    @FocusState private var focus: String?
    @State private var lastFocus: String?
    @State private var showAdd = false

    var body: some View {
        NavigationStack {
            List {
                Section {
                    ForEach(model.codes, id: \.self) { code in
                        CurrencyRow(
                            code: code,
                            text: Binding(get: { model.text(code) }, set: { model.edit(code, $0) }),
                            rateText: model.rateText(code),
                            isFocused: focus == code,
                            focus: $focus
                        )
                        .deleteDisabled(model.codes.count <= 1)
                    }
                    .onDelete { model.remove(at: $0) }
                    .onMove { model.move(from: $0, to: $1) }
                } footer: {
                    Text(model.statusText)
                        .font(.footnote)
                        .foregroundStyle(model.statusIsWarning ? Color.orange : Color.secondary)
                        .padding(.top, 4)
                }

                Section {
                    Button {
                        focus = nil
                        showAdd = true
                    } label: {
                        Label("添加货币", systemImage: "plus.circle.fill")
                            .font(.body.weight(.medium))
                    }
                }
            }
            .listStyle(.insetGrouped)
            .scrollDismissesKeyboard(.interactively)
            .refreshable { await model.refresh() }
            .navigationTitle("汇率换算")
            .toolbar {
                ToolbarItem(placement: .navigationBarLeading) {
                    EditButton()
                }
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button {
                        Task { await model.refresh() }
                    } label: {
                        if model.fetching {
                            ProgressView()
                        } else {
                            Image(systemName: "arrow.clockwise")
                        }
                    }
                    .disabled(model.fetching)
                    .accessibilityLabel("刷新汇率")
                }
                ToolbarItemGroup(placement: .keyboard) {
                    ForEach(["+", "−", "×", "÷"], id: \.self) { op in
                        Button(op) { insert(op) }
                            .font(.title3.weight(.medium))
                            .frame(minWidth: 36)
                    }
                    Button("=") { if let f = focus { model.commit(f) } }
                        .font(.title3.weight(.medium))
                        .frame(minWidth: 36)
                    Spacer()
                    Button("完成") { focus = nil }
                        .font(.body.weight(.semibold))
                }
            }
            .sheet(isPresented: $showAdd) {
                AddCurrencyView(available: model.available) { model.add($0) }
            }
        }
        .onChange(of: focus) { newValue in
            if let old = lastFocus, old != newValue { model.commit(old) }
            lastFocus = newValue
        }
        .onChange(of: scenePhase) { phase in
            if phase == .active { Task { await model.refreshIfStale() } }
        }
        .task {
            // 打开时获取最新汇率，之后在前台每分钟检查一次是否已超过 30 分钟
            while !Task.isCancelled {
                await model.refreshIfStale()
                try? await Task.sleep(nanoseconds: 60 * 1_000_000_000)
            }
        }
        // 点进输入框时全选，方便直接输入新数字
        .onReceive(NotificationCenter.default.publisher(for: UITextField.textDidBeginEditingNotification)) { note in
            guard let field = note.object as? UITextField, field.keyboardType == .decimalPad else { return }
            DispatchQueue.main.async { field.selectAll(nil) }
        }
    }

    private func insert(_ op: String) {
        guard let f = focus else { return }
        model.edit(f, model.text(f) + op)
    }
}

struct CurrencyRow: View {
    let code: String
    @Binding var text: String
    let rateText: String
    let isFocused: Bool
    var focus: FocusState<String?>.Binding

    var body: some View {
        HStack(spacing: 12) {
            CurrencyBadge(code: code, size: 40)

            VStack(alignment: .leading, spacing: 2) {
                Text(code)
                    .font(.headline)
                Text(Currencies.name(code))
                    .font(.caption)
                    .foregroundStyle(.secondary)
                    .lineLimit(1)
            }
            .layoutPriority(1)

            VStack(alignment: .trailing, spacing: 2) {
                TextField("0", text: $text)
                    .keyboardType(.decimalPad)
                    .multilineTextAlignment(.trailing)
                    .font(.system(size: 24, weight: .semibold, design: .rounded))
                    .monospacedDigit()
                    .foregroundStyle(isFocused ? Color.accentColor : Color.primary)
                    .minimumScaleFactor(0.5)
                    .focused(focus, equals: code)
                Text(rateText)
                    .font(.caption2)
                    .foregroundStyle(.secondary)
                    .lineLimit(1)
            }
        }
        .padding(.vertical, 6)
        .contentShape(Rectangle())
        .onTapGesture { focus.wrappedValue = code }
    }
}

struct CurrencyBadge: View {
    let code: String
    let size: CGFloat

    var body: some View {
        let symbol = Currencies.symbol(code)
        Circle()
            .fill(Currencies.color(code).gradient)
            .frame(width: size, height: size)
            .overlay(
                Text(symbol)
                    .font(.system(size: size * (symbol.count >= 3 ? 0.28 : symbol.count == 2 ? 0.34 : 0.42), weight: .semibold))
                    .foregroundStyle(.white)
                    .lineLimit(1)
                    .minimumScaleFactor(0.6)
                    .padding(2)
            )
    }
}
