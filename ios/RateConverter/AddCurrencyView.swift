import SwiftUI

/// 添加货币：可按中文名、代码或符号搜索
struct AddCurrencyView: View {
    let available: [String]
    let onPick: (String) -> Void
    @Environment(\.dismiss) private var dismiss
    @State private var query = ""

    private var filtered: [String] {
        let q = query.trimmingCharacters(in: .whitespaces).uppercased()
        if q.isEmpty { return available }
        return available.filter {
            $0.contains(q) || Currencies.name($0).uppercased().contains(q) || Currencies.symbol($0).uppercased() == q
        }
    }

    var body: some View {
        NavigationStack {
            List(filtered, id: \.self) { code in
                Button {
                    onPick(code)
                    dismiss()
                } label: {
                    HStack(spacing: 12) {
                        CurrencyBadge(code: code, size: 34)
                        VStack(alignment: .leading, spacing: 1) {
                            Text(Currencies.name(code)).foregroundStyle(.primary)
                            Text(code).font(.caption).foregroundStyle(.secondary)
                        }
                        Spacer()
                        Image(systemName: "plus").foregroundStyle(.tint)
                    }
                }
            }
            .listStyle(.plain)
            .overlay {
                if filtered.isEmpty {
                    Text("没有找到“\(query)”").foregroundStyle(.secondary)
                }
            }
            .searchable(text: $query, placement: .navigationBarDrawer(displayMode: .always), prompt: "搜索名称或代码，如 欧元 / EUR")
            .navigationTitle("添加货币")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("取消") { dismiss() }
                }
            }
        }
    }
}
