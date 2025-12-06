# TextMeshPro Migration Guide

## What Changed
Updated `BalanceTestUI.cs` to use **TextMeshPro** components instead of legacy Unity UI:

- `InputField` → `TMP_InputField`
- `Text` → `TMP_Text`

## Next Steps in Unity Editor

1. **Open** `BalanceTestScene.unity`
2. **Select** the Canvas object
3. **Replace UI Components**:
   - Delete old `InputField` (Account Address)
   - Add **TextMeshPro - InputField** (Right-click → UI → TextMeshPro - Input Field)
   - Delete old `Text` components (Status, Balance CSPR, Balance Motes, Error)
   - Add **TextMeshPro - Text** components for each
4. **Re-assign** in `BalanceTestUI` component:
   - Account Address Input → new TMP InputField
   - Status Text → new TMP Text
   - Balance Cspr Text → new TMP Text
   - Balance Motes Text → new TMP Text
   - Error Text → new TMP Text
5. **Import TMP Essentials** if prompted (Window → TextMeshPro → Import TMP Essential Resources)
6. **Save** the scene
7. **Test** by clicking Play

## Benefits
✅ Sharper text rendering
✅ Better performance
✅ Modern Unity standard
✅ Rich text formatting support
